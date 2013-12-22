'/-----------------------------------------------------------------------------\
'| ########################################################################### |
'| #                                                                         # |
'| # B@Z Socket - Class facilitant la gestion des sockets en VB.Net          # |
'| #                                                                         # |
'| # Copyright (C) 2006 - Bab                                                # |
'| #                                                                         # |
'| # E-mail : vincentbab@freesurf.fr                                         # |
'| #                                                                         # |
'| ########################################################################### |
'\-----------------------------------------------------------------------------/

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.ComponentModel

Namespace Server

    Public Delegate Sub DataArrivalEventHandler(ByVal sender As Object, ByVal data() As Byte)
    Public Delegate Sub AcceptedEventHandler(ByVal sender As Object, ByVal Request As AcceptRequest)
    Public Delegate Sub ErrorEventHandler(ByVal sender As Object, ByVal ex As Exception)
    Public Delegate Sub SendProgressEventHandler(ByVal sender As Object, ByVal current As Integer, ByVal total As Integer)
    Public Delegate Sub SendCompleteEventHandler(ByVal sender As Object, ByVal total As Integer)
    Public Delegate Sub StateChangedEventHandler(ByVal sender As Object, ByVal state As BazSocketState)

    Public Class BazSocket

#Region " Membres "
        'TODO: Metre en property ?
        Private Const BACKLOG As Integer = 8

        Private m_RecvSize As Integer   'Taille du buffer pour recevoir les données
        Private m_RecvBuffer() As Byte  'Buffer pour recevoir les données

        Private m_SendSize As Integer   'Taille du buffer pour envoyer les données
        Private m_SendBuffer() As Byte  'Buffer pour envoyer les données

        Private m_Sync As ISynchronizeInvoke 'Merci à Xya ;)

        Private m_Socket As Socket
        Private m_Stream As NetworkStream
        Private m_State As BazSocketState  'Etat du socket

        Private m_LocalEP As IPEndPoint
        Private m_RemoteEP As IPEndPoint
        Private m_RemotePort As Integer

        Private m_Init As Boolean       'Est-ce que le socket est initialisé ou pas ?
        'Voir Property AsyncEvent
        Private m_AsyncEvent As Boolean
        'Voir Property AlwaysRaiseClose
        Private m_AlwaysRaiseClose As Boolean

        Private AcceptSync As Object '}
        Private SendSync As Object   '} Object servant pour le SyncLock des threads
        Private RecvSync As Object   '}
#End Region

#Region " Constructeur "
        'Constructeur par default
        Public Sub New()
            Me.New(Nothing)
        End Sub

        Public Sub New(ByRef Sync As ISynchronizeInvoke)
            m_Sync = Sync
            InitInvoke()


            'Valeur par default
            m_AlwaysRaiseClose = False
            m_AsyncEvent = True

            AcceptSync = New Object
            SendSync = New Object
            RecvSync = New Object

            CreateSocket()
            InitBuffers()

            OnStateChanged(BazSocketState.Disconnected)
        End Sub

        'Creation du socket pour accepter une nouvelle connection
        Public Sub New(ByRef Sync As ISynchronizeInvoke, ByRef Request As AcceptRequest)
            If Request.Sock Is Nothing OrElse Request.Sock.Connected = False OrElse Request.Stream Is Nothing Then
                m_Init = False
                Return
            End If

            m_Sync = Sync
            InitInvoke()

            m_AlwaysRaiseClose = False
            m_AsyncEvent = True

            m_Socket = Request.Sock
            m_Stream = Request.Stream
            m_State = BazSocketState.Connected
            InitBuffers()

            AcceptSync = New Object()
            SendSync = New Object()
            RecvSync = New Object()

            m_Init = True

            OnStateChanged(BazSocketState.Connected)

            'On commence a recevoir les données qui pourraient arriver.
            m_Stream.BeginRead(m_RecvBuffer, 0, m_RecvSize, New AsyncCallback(AddressOf Receive_CallBack), m_Socket)
        End Sub
#End Region

#Region " Events "
        Public Event StateChanged As StateChangedEventHandler
        Public Event Connected As EventHandler
        Public Event ConnectionFailed As ErrorEventHandler
        Public Event Listening As EventHandler
        Public Event ListenFailed As ErrorEventHandler
        Public Event Accepted As AcceptedEventHandler
        Public Event AcceptFailed As ErrorEventHandler
        Public Event DataArrival As DataArrivalEventHandler
        Public Event SendProgress As SendProgressEventHandler
        Public Event SendComplete As SendCompleteEventHandler
        Public Event Closed As EventHandler
        Public Event ThreadException As ErrorEventHandler
#End Region

#Region " -- Cross-Threading -- "
        'Merci à Xya pour son aide qui ma permit de régler le problème de Cross-Threading !!

        'Délégués pour appeler les sub grace a m_Sync.Invoke
        Private Delegate Sub _StateChanged(ByVal state As BazSocketState)
        Private Delegate Sub _Connected()
        Private Delegate Sub _ConnectionFailed(ByVal ex As Exception)
        Private Delegate Sub _Listening()
        Private Delegate Sub _ListenFailed(ByVal ex As Exception)
        Private Delegate Sub _Accepted(ByVal Request As AcceptRequest)
        Private Delegate Sub _AcceptFailed(ByVal ex As Exception)
        Private Delegate Sub _DataArrival(ByVal data() As Byte)
        Private Delegate Sub _SendProgress(ByVal current As Integer, ByVal total As Integer)
        Private Delegate Sub _SendComplete(ByVal total As Integer)
        Private Delegate Sub _Close()
        Private Delegate Sub _ThreadException(ByVal ex As Exception)

        'Les Délégués
        Private CallStateChanged As _StateChanged
        Private CallConnected As _Connected
        Private CallConnectionFailed As _ConnectionFailed
        Private CallListening As _Listening
        Private CallListenFailed As _ListenFailed
        Private CallAccepted As _Accepted
        Private CallAcceptFailed As _AcceptFailed
        Private CallDataArrival As _DataArrival
        Private CallSendProgress As _SendProgress
        Private CallSendComplete As _SendComplete
        Private CallClose As _Close
        Private CallThreadException As _ThreadException

        Private Sub InitInvoke()
            CallStateChanged = New _StateChanged(AddressOf Sync_StateChanged)
            CallConnected = New _Connected(AddressOf Sync_Connected)
            CallConnectionFailed = New _ConnectionFailed(AddressOf Sync_ConnectionFailed)
            CallListening = New _Listening(AddressOf Sync_Listening)
            CallListenFailed = New _ListenFailed(AddressOf Sync_ListenFailed)
            CallAccepted = New _Accepted(AddressOf Sync_Accepted)
            CallAcceptFailed = New _AcceptFailed(AddressOf Sync_AcceptFailed)
            CallDataArrival = New _DataArrival(AddressOf Sync_DataArrival)
            CallSendProgress = New _SendProgress(AddressOf Sync_SendProgress)
            CallSendComplete = New _SendComplete(AddressOf Sync_SendComplete)
            CallClose = New _Close(AddressOf Sync_Closed)
            CallThreadException = New _ThreadException(AddressOf Sync_ThreadException)
        End Sub

        'Sert a regarder si on a besoin d'apeller l'Event depuis un autre thread.
        Private Function InvokeRequired() As Boolean
            If m_Sync IsNot Nothing AndAlso m_Sync.InvokeRequired Then
                Return True
            Else
                Return False
            End If
        End Function

        'Evenements appelé par les threads.
        Private Sub OnStateChanged(ByVal state As BazSocketState)
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallStateChanged, New Object() {state})
                    Else
                        m_Sync.Invoke(CallStateChanged, New Object() {state})
                    End If
                Else
                    Sync_StateChanged(state)
                End If
            Catch ex As Exception
                OnThreadException(ex)
            End Try
        End Sub
        Private Sub OnConnected()
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallConnected, Nothing)
                    Else
                        m_Sync.Invoke(CallConnected, Nothing)
                    End If
                Else
                    Sync_Connected()
                End If
            Catch ex As Exception
                OnThreadException(ex)
            End Try
        End Sub
        Private Sub OnConnectionFailed(ByVal ex As Exception)
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallConnectionFailed, New Object() {ex})
                    Else
                        m_Sync.Invoke(CallConnectionFailed, New Object() {ex})
                    End If
                Else
                    Sync_ConnectionFailed(ex)
                End If
            Catch exp As Exception
                OnThreadException(exp)
            End Try
        End Sub
        Private Sub OnListening()
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallListening, Nothing)
                    Else
                        m_Sync.Invoke(CallListening, Nothing)
                    End If
                Else
                    Sync_Listening()
                End If
            Catch ex As Exception
                OnThreadException(ex)
            End Try
        End Sub
        Private Sub OnListenFailed(ByVal ex As Exception)
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallListenFailed, New Object() {ex})
                    Else
                        m_Sync.Invoke(CallListenFailed, New Object() {ex})
                    End If
                Else
                    Sync_ListenFailed(ex)
                End If
            Catch exp As Exception
                OnThreadException(exp)
            End Try
        End Sub
        Private Sub OnAccepted(ByVal Request As AcceptRequest)
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallAccepted, New Object() {Request})
                    Else
                        m_Sync.Invoke(CallAccepted, New Object() {Request})
                    End If
                Else
                    Sync_Accepted(Request)
                End If
            Catch ex As Exception
                OnThreadException(ex)
            End Try
        End Sub
        Private Sub OnAcceptFailed(ByVal ex As Exception)
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallAcceptFailed, New Object() {ex})
                    Else
                        m_Sync.BeginInvoke(CallAcceptFailed, New Object() {ex})
                    End If
                Else
                    Sync_AcceptFailed(ex)
                End If
            Catch exp As Exception
                OnThreadException(exp)
            End Try
        End Sub
        Private Sub OnDataArrival(ByVal data() As Byte)
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallDataArrival, New Object() {data})
                    Else
                        m_Sync.Invoke(CallDataArrival, New Object() {data})
                    End If
                Else
                    Sync_DataArrival(data)
                End If
            Catch ex As Exception
                OnThreadException(ex)
            End Try
        End Sub
        Private Sub OnSendProgress(ByVal current As Integer, ByVal total As Integer)
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallSendProgress, New Object() {current, total})
                    Else
                        m_Sync.Invoke(CallSendProgress, New Object() {current, total})
                    End If
                Else
                    Sync_SendProgress(current, total)
                End If
            Catch ex As Exception
                OnThreadException(ex)
            End Try
        End Sub
        Private Sub OnSendComplete(ByVal total As Integer)
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallSendComplete, New Object() {total})
                    Else
                        m_Sync.Invoke(CallSendComplete, New Object() {total})
                    End If
                Else
                    Sync_SendComplete(total)
                End If
            Catch ex As Exception
                OnThreadException(ex)
            End Try
        End Sub
        Private Sub OnClosed()
            Try
                If InvokeRequired() Then
                    If m_AsyncEvent Then
                        m_Sync.BeginInvoke(CallClose, Nothing)
                    Else
                        m_Sync.Invoke(CallClose, Nothing)
                    End If
                Else
                    Sync_Closed()
                End If
            Catch ex As Exception
                OnThreadException(ex)
            End Try
        End Sub
        Private Sub OnThreadException(ByVal ex As Exception)
            If InvokeRequired() Then
                If m_AsyncEvent Then
                    m_Sync.BeginInvoke(CallThreadException, New Object() {ex})
                Else
                    m_Sync.Invoke(CallThreadException, New Object() {ex})
                End If
            Else
                Sync_ThreadException(ex)
            End If
        End Sub

        'Fonction appeler dans le thread m_Sync qui vont declencher les Events
        Private Sub Sync_StateChanged(ByVal state As BazSocketState)
            RaiseEvent StateChanged(Me, state)
        End Sub
        Private Sub Sync_Connected()
            RaiseEvent Connected(Me, EventArgs.Empty)
        End Sub
        Private Sub Sync_ConnectionFailed(ByVal ex As Exception)
            RaiseEvent ConnectionFailed(Me, ex)
        End Sub
        Private Sub Sync_Listening()
            RaiseEvent Listening(Me, EventArgs.Empty)
        End Sub
        Private Sub Sync_ListenFailed(ByVal ex As Exception)
            RaiseEvent ListenFailed(Me, ex)
        End Sub
        Private Sub Sync_Accepted(ByVal Request As AcceptRequest)
            RaiseEvent Accepted(Me, Request)
        End Sub
        Private Sub Sync_AcceptFailed(ByVal ex As Exception)
            RaiseEvent AcceptFailed(Me, ex)
        End Sub
        Private Sub Sync_DataArrival(ByVal data() As Byte)
            RaiseEvent DataArrival(Me, data)
        End Sub
        Private Sub Sync_SendProgress(ByVal current As Integer, ByVal total As Integer)
            RaiseEvent SendProgress(Me, current, total)
        End Sub
        Private Sub Sync_SendComplete(ByVal total As Integer)
            RaiseEvent SendComplete(Me, total)
        End Sub
        Private Sub Sync_Closed()
            RaiseEvent Closed(Me, EventArgs.Empty)
        End Sub
        Private Sub Sync_ThreadException(ByVal ex As Exception)
            RaiseEvent ThreadException(Me, ex)
        End Sub
#End Region

#Region " Proprietés "
        Public ReadOnly Property Initialised() As Boolean
            Get
                Return m_Init
            End Get
        End Property

        'Etat du socket
        Public ReadOnly Property State() As BazSocketState
            Get
                Return m_State
            End Get
        End Property

        Public Property LocalEP() As IPEndPoint
            Get
                If m_State <> BazSocketState.Disconnected Then
                    Return CType(m_Socket.LocalEndPoint, IPEndPoint)
                Else
                    Return m_LocalEP
                End If
            End Get
            Set(ByVal Value As IPEndPoint)
                If m_State = BazSocketState.Disconnected Then
                    m_LocalEP = Value
                End If
            End Set
        End Property
        Public Property RemoteEP() As IPEndPoint
            Get
                If m_State = BazSocketState.Connected OrElse m_State = BazSocketState.Connecting Then
                    Return CType(m_Socket.RemoteEndPoint, IPEndPoint)
                Else
                    Return m_RemoteEP
                End If
            End Get
            Set(ByVal Value As IPEndPoint)
                If m_State = BazSocketState.Disconnected Then
                    m_RemoteEP = Value
                End If
            End Set
        End Property

        'Si True,  L'evenement Close est déclenché quand vous appelez Disconnect.
        'Si False, L'evenement Close est déclenché uniquement quand la connection est fermé a distance. Comme Winsock de VB6 (Defaut)
        Public Property AlwaysRaiseClose() As Boolean
            Get
                Return m_AlwaysRaiseClose
            End Get
            Set(ByVal value As Boolean)
                m_AlwaysRaiseClose = value
            End Set
        End Property

        'Si True,  Les Evenements sont lancés chaque fois qu'il ce produise. Comme Winsock de VB6 (Defaut)
        'Si False, Les Evenements sont mit en attente s'il y en a deja un qui est entrin de s'executer. Ca permet de traiter les données dans l'ordre ou elle arrive.
        Public Property AsyncEvent() As Boolean
            Get
                Return m_AsyncEvent
            End Get
            Set(ByVal value As Boolean)
                m_AsyncEvent = value
            End Set
        End Property

        'Permet de regler la taille des buffers d'envoie et de reception.
        Public Property SendBufferSize() As Integer
            Get
                Return m_SendSize
            End Get
            Set(ByVal value As Integer)
                If m_Init AndAlso m_State = BazSocketState.Disconnected Then
                    m_SendSize = value
                    m_Socket.SendBufferSize = m_SendSize
                    ReDim m_SendBuffer(m_SendSize - 1)
                End If
            End Set
        End Property
        Public Property ReceiveBufferSize() As Integer
            Get
                Return m_RecvSize
            End Get
            Set(ByVal value As Integer)
                If m_Init AndAlso m_State = BazSocketState.Disconnected Then
                    m_RecvSize = value
                    m_Socket.ReceiveBufferSize = m_RecvSize
                    ReDim m_RecvBuffer(m_RecvSize - 1)
                End If
            End Set
        End Property

        'Merci à Xya ;)
        'Pour le Cross-Threading
        Public Property SynchronizingObject() As ISynchronizeInvoke
            Get
                Return m_Sync
            End Get
            Set(ByVal value As ISynchronizeInvoke)
                m_Sync = value
            End Set
        End Property
#End Region

#Region " Public Methodes "
        ''' <summary>
        ''' Connecte le socket à un hôte distant
        ''' </summary>
        ''' <param name="RemoteHost">L'hôte auquel on se connecte</param>
        ''' <param name="RemotePort">Le port sur lequel on se connecte</param>
        ''' <remarks></remarks>
        Public Sub Connect(ByVal RemoteHost As String, ByVal RemotePort As Integer)
            If m_State = BazSocketState.Disconnected AndAlso m_Init Then
                Try
                    m_RemotePort = RemotePort

                    ' Fait la resolution de nom uniquement quand le RemoteHost n'est pas deja une IP ^^
                    Dim ip As New IPAddress(0)
                    If (IPAddress.TryParse(RemoteHost, ip)) Then
                        Me.Connect(Nothing, New IPEndPoint(ip, RemotePort))
                    Else
                        m_State = BazSocketState.Resolving
                        OnStateChanged(BazSocketState.Resolving)
                        ' C'est mieu d'utilisé BeginGetHostEntry() pour ne pas bloqué le thread courant
                        Dns.BeginGetHostEntry(RemoteHost, New AsyncCallback(AddressOf GetHostEntry_CallBack), m_Socket)
                    End If
                Catch ex As Exception
                    m_State = BazSocketState.Disconnected
                    OnStateChanged(BazSocketState.Disconnected)
                    OnConnectionFailed(ex)
                End Try
            End If
        End Sub

        ''' <summary>
        ''' Connecte le socket en utilisant le LocalEP et RemoteEP du socket
        ''' </summary>
        ''' <remarks>Vous devez renseigner les propriétés LocalEP et RemoteEP du socket avant d'utiliser cette Methode</remarks>
        Public Sub Connect()
            Me.Connect(m_LocalEP, m_RemoteEP)
        End Sub

        ''' <summary>
        ''' Connecte le socket à un hôte distant
        ''' </summary>
        ''' <param name="RemoteEP">Adresse sur laquelle on se connecte</param>
        ''' <remarks></remarks>
        Public Sub Connect(ByVal RemoteEP As IPEndPoint)
            Me.Connect(Nothing, RemoteEP)
        End Sub

        ''' <summary>
        ''' Connecte le socket à un hôte distant
        ''' </summary>
        ''' <param name="LocalEP">Adresse sur laquelle on Bind le socket</param>
        ''' <param name="RemoteEP">Adresse sur laquelle on se connecte</param>
        ''' <remarks></remarks>
        Public Sub Connect(ByVal LocalEP As IPEndPoint, ByVal RemoteEP As IPEndPoint)
            If m_State = BazSocketState.Disconnected AndAlso m_Init AndAlso RemoteEP IsNot Nothing Then
                Try
                    m_RemoteEP = RemoteEP
                    m_LocalEP = LocalEP
                    m_State = BazSocketState.Connecting
                    OnStateChanged(BazSocketState.Connecting)

                    If m_LocalEP IsNot Nothing Then
                        m_Socket.Bind(m_LocalEP)
                    End If

                    m_Socket.BeginConnect(m_RemoteEP, New AsyncCallback(AddressOf Connect_CallBack), m_Socket)
                Catch ex As Exception
                    m_State = BazSocketState.Disconnected
                    OnStateChanged(BazSocketState.Disconnected)
                    OnConnectionFailed(ex)
                End Try
            End If
        End Sub

        ''' <summary>
        ''' Ferme la conexion actuel.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Disconnect()
            If m_State <> BazSocketState.Disconnected Then

                If m_AlwaysRaiseClose Then
                    DisconnectSocket(True)
                Else
                    DisconnectSocket(False)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Ferme la conexion actuel. Le socket est ensuite inutilisable
        ''' </summary>
        ''' <remarks>N'essayez pas de réutiliser le socket apres avoir appeler cette methode</remarks>
        Public Sub Close()
            If m_State <> BazSocketState.Disconnected AndAlso m_AlwaysRaiseClose Then
                CloseSocket()
                OnClosed()
            Else
                CloseSocket()
            End If
        End Sub

        ''' <summary>
        ''' Met le socket en écoute en utilisant le LocalEP
        ''' </summary>
        ''' <remarks>Vous devez renseigner la prorpiété LocalEP avant d'appeler cette methode</remarks>
        Public Sub Listen()
            Me.Listen(m_LocalEP)
        End Sub

        ''' <summary>
        ''' Met le socket en écoute
        ''' </summary>
        ''' <param name="LocalIP">Addresse sur laquelle on écoute</param>
        ''' <param name="LocalPort">Port sur lequel on ecoute</param>
        ''' <remarks></remarks>
        Public Sub Listen(ByVal LocalIP As String, ByVal LocalPort As Integer)
            Try
                Dim IpAddr As IPAddress = IPAddress.Parse(LocalIP)
                m_LocalEP = New IPEndPoint(IpAddr, LocalPort)
            Catch ex As Exception
                OnListenFailed(ex)
                Return
            End Try


            Me.Listen(m_LocalEP)
        End Sub

        ''' <summary>
        ''' Met le socket en écoute sur toute les addresse de la machine
        ''' </summary>
        ''' <param name="LocalPort">Port sur lequel on écoute</param>
        ''' <remarks></remarks>
        Public Sub Listen(ByVal LocalPort As Integer)
            Try
                m_LocalEP = New IPEndPoint(IPAddress.Any, LocalPort)
            Catch ex As Exception
                OnListenFailed(ex)
                Return
            End Try

            Me.Listen(m_LocalEP)
        End Sub

        ''' <summary>
        ''' Met le socket en écoute
        ''' </summary>
        ''' <param name="LocalEP">EndPoint sur lequel on écoute</param>
        ''' <remarks></remarks>
        Public Sub Listen(ByVal LocalEP As IPEndPoint)
            If m_State = BazSocketState.Disconnected AndAlso m_Init Then
                Try
                    m_Socket.Bind(LocalEP)

                    m_Socket.Listen(BACKLOG)
                    m_Socket.BeginAccept(New AsyncCallback(AddressOf Accept_CallBack), m_Socket)

                    m_State = BazSocketState.Listening
                    OnStateChanged(BazSocketState.Listening)
                    OnListening()
                Catch ex As Exception
                    m_State = BazSocketState.Disconnected
                    OnStateChanged(BazSocketState.Disconnected)
                    OnListenFailed(ex)
                End Try

            End If
        End Sub

        ''' <summary>
        ''' Envoie des données à travers le socket
        ''' </summary>
        ''' <param name="data">Tableau de byte à envoyer</param>
        ''' <remarks></remarks>
        Public Sub Send(ByVal data() As Byte)
            If data Is Nothing Then Return
            If data.Length = 0 Then Return

            If m_State = BazSocketState.Connected Then
                m_Stream.BeginWrite(data, 0, data.Length, New AsyncCallback(AddressOf Send_CallBack), m_Socket)
            End If
        End Sub

        ''' <summary>
        ''' Envoie des données à travers le socket
        ''' </summary>
        ''' <param name="data">Chaine de caractere à envoyer</param>
        ''' <remarks></remarks>
        Public Sub Send(ByVal data As String)
            If m_State = BazSocketState.Connected Then
                Dim ByteData() As Byte = Encoding.Default.GetBytes(data)
                Me.Send(ByteData)
            End If
        End Sub
#End Region

#Region " CallBack Methodes "
        Private Sub GetHostEntry_CallBack(ByVal Ar As IAsyncResult)
            Dim AsyncSocket As Socket = DirectCast(Ar.AsyncState, Socket)

            'Regarde si le socket existe encore, sinon on quite
            If Not Object.ReferenceEquals(AsyncSocket, m_Socket) Then
                Return
            End If

            Try
                Dim HostEntry As IPHostEntry = Dns.EndGetHostEntry(Ar)
                'On boucle sur la liste des address retourné pour en trouver une en IPV4 car la 1er de la liste est une IPV6 sous Vista ;)
                For Each Ip As IPAddress In HostEntry.AddressList
                    If Ip.AddressFamily = AddressFamily.InterNetwork Then 'IPV4
                        m_RemoteEP = New IPEndPoint(Ip, m_RemotePort)
                        m_State = BazSocketState.Disconnected
                        Me.Connect(m_LocalEP, m_RemoteEP)
                        Return
                    End If
                Next

                ' Si on arrive ici, c'est qu'il n'y a pas d'address IPV4 pour cet Host.
                m_State = BazSocketState.Disconnected
                OnStateChanged(BazSocketState.Disconnected)
                OnConnectionFailed(New SocketException(SocketError.HostNotFound))

            Catch ex As Exception
                m_State = BazSocketState.Disconnected
                OnStateChanged(BazSocketState.Disconnected)
                OnConnectionFailed(ex)
            End Try


        End Sub

        Private Sub Connect_CallBack(ByVal Ar As IAsyncResult)
            Dim AsyncSocket As Socket = DirectCast(Ar.AsyncState, Socket)

            'Regarde si le socket Existe encore, Sinon on quite
            If Not Object.ReferenceEquals(AsyncSocket, m_Socket) Then
                Return
            End If

            Try
                m_Socket.EndConnect(Ar)
                If Not m_Socket.Connected Then
                    Throw New Exception()
                End If

                'Normalement ici on est connecter.
                m_Stream = New NetworkStream(m_Socket) 'Crée notre stream
                m_State = BazSocketState.Connected
                OnStateChanged(BazSocketState.Connected)
                OnConnected()

                m_Stream.BeginRead(m_RecvBuffer, 0, m_RecvSize, New AsyncCallback(AddressOf Receive_CallBack), m_Socket)
            Catch ex As Exception
                m_State = BazSocketState.Disconnected
                OnStateChanged(BazSocketState.Disconnected)
                OnConnectionFailed(ex)
                Return
            End Try
        End Sub

        Private Sub Accept_CallBack(ByVal Ar As IAsyncResult)
            Dim AsyncSocket As Socket = DirectCast(Ar.AsyncState, Socket)
            Dim NewSocket As Socket, NewStream As NetworkStream

            SyncLock AcceptSync
                'Regarde si le socket Existe encore, Sinon on quite
                If Not Object.ReferenceEquals(AsyncSocket, m_Socket) Then
                    Return
                End If


                Try
                    NewSocket = m_Socket.EndAccept(Ar)
                    If Not NewSocket.Connected Then
                        NewSocket.Close()
                        NewSocket = Nothing
                        Throw New Exception()
                    End If

                    NewStream = New NetworkStream(NewSocket) 'Crée notre stream

                    OnAccepted(New AcceptRequest(NewSocket, NewStream))

                    m_Socket.BeginAccept(New AsyncCallback(AddressOf Accept_CallBack), m_Socket)
                Catch ex As Exception
                    m_State = BazSocketState.Disconnected
                    OnStateChanged(BazSocketState.Disconnected)
                    OnAcceptFailed(ex)
                    Return
                End Try
            End SyncLock
        End Sub

        Private Sub Receive_CallBack(ByVal Ar As IAsyncResult)
            Dim AsyncSocket As Socket = DirectCast(Ar.AsyncState, Socket)
            Dim RecvSize As Integer

            SyncLock RecvSync

                'Regarde si le socket Existe encore, Sinon on quite
                If Not Object.ReferenceEquals(AsyncSocket, m_Socket) Then
                    Return
                End If

                Try
                    RecvSize = m_Stream.EndRead(Ar)
                    If RecvSize <> 0 Then
                        Dim ByteData(RecvSize - 1) As Byte
                        Array.Copy(m_RecvBuffer, ByteData, RecvSize)

                        OnDataArrival(ByteData)


                        If m_Stream.CanRead Then
                            m_Stream.BeginRead(m_RecvBuffer, 0, m_RecvSize, New AsyncCallback(AddressOf Receive_CallBack), m_Socket)
                        Else
                            DisconnectSocket(True)
                        End If

                    Else
                        DisconnectSocket(True)
                    End If
                Catch ex As Exception
                    DisconnectSocket(True)
                End Try
            End SyncLock
        End Sub

        Private Sub Send_CallBack(ByVal Ar As IAsyncResult)
            Dim AsyncSocket As Socket = DirectCast(Ar.AsyncState, Socket)

            SyncLock SendSync
                If Not Object.ReferenceEquals(AsyncSocket, m_Socket) Then
                    Return
                End If

                Try
                    m_Stream.EndWrite(Ar)
                Catch ex As Exception
                    DisconnectSocket(True)
                End Try
            End SyncLock
        End Sub
#End Region

#Region " Fonctions privées "
        'Detruit le Socket et en crée un nouveau
        Private Sub DisconnectSocket(ByVal RaiseClose As Boolean)
            CloseSocket()
            CreateSocket()

            OnStateChanged(BazSocketState.Disconnected)
            If RaiseClose Then
                OnClosed()
            End If
        End Sub
        'Detruit le Socket completement (Il est ensuite inutilisable)
        Private Sub CloseSocket()
            If m_Socket Is Nothing Then Return

            If m_Socket.Connected Then
                m_Socket.Shutdown(SocketShutdown.Both)
                If m_Stream IsNot Nothing Then m_Stream.Close()
            End If
            If m_Init Then
                m_Socket.Close()
                m_Socket = Nothing
                'On a fermer le socket il n'est donc plus initialisé !
                m_Init = False
            End If
            m_State = BazSocketState.Disconnected
        End Sub
        'Crée un nouveau socket
        Private Sub CreateSocket()
            Try
                m_Socket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                ' Defini la taille des buffer de lecture et d'ecriture
                'm_Socket.ReceiveBufferSize = m_RecvSize
                'm_Socket.SendBufferSize = m_SendSize
                m_State = BazSocketState.Disconnected
            Catch ex As Exception
                m_Init = False
                Return
            End Try

            If m_Socket Is Nothing Then
                m_Init = False
            Else
                m_Init = True
            End If
        End Sub
        'Initialise les buffers d'envoie et de reception
        Private Sub InitBuffers()
            If m_Socket IsNot Nothing Then
                m_RecvSize = m_Socket.ReceiveBufferSize
                ReDim m_RecvBuffer(m_RecvSize - 1)

                m_SendSize = m_Socket.SendBufferSize
                ReDim m_SendBuffer(m_RecvSize - 1)
            End If
        End Sub
#End Region

    End Class

    Public Enum BazSocketState
        Disconnected
        Resolving
        Listening
        Connecting
        Connected
    End Enum

    Public Class AcceptRequest
        Private m_Socket As Socket
        Private m_Stream As NetworkStream

        Friend Sub New(ByVal sock As Socket, ByVal stream As NetworkStream)
            m_Socket = sock
            m_Stream = stream
        End Sub

        Friend ReadOnly Property Sock() As Socket
            Get
                Return m_Socket
            End Get
        End Property
        Friend ReadOnly Property Stream() As NetworkStream
            Get
                Return m_Stream
            End Get
        End Property

        Public ReadOnly Property LocalEP() As IPEndPoint
            Get
                Return CType(m_Socket.LocalEndPoint, IPEndPoint)
            End Get
        End Property
        Public ReadOnly Property RemoteEP() As IPEndPoint
            Get
                Return CType(m_Socket.RemoteEndPoint, IPEndPoint)
            End Get
        End Property
    End Class

End Namespace