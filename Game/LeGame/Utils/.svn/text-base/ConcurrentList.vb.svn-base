Imports System.Collections.Concurrent

Namespace Utils

    Public Class ConcurrentList(Of T)
        Implements IList(Of T)

        Private ReadOnly underlyingList As New List(Of T)()
        Private ReadOnly m_syncRoot As New Object()
        Private ReadOnly underlyingQueue As ConcurrentQueue(Of T)
        Private requiresSync As Boolean
        Private isDirty As Boolean

        Public Sub New()
            underlyingQueue = New ConcurrentQueue(Of T)()
        End Sub

        Public Sub New(ByVal items As IEnumerable(Of T))
            underlyingQueue = New ConcurrentQueue(Of T)(items)
        End Sub

        Private Sub UpdateLists()
            If Not isDirty Then
                Return
            End If
            SyncLock m_syncRoot
                requiresSync = True
                Dim temp As T
                While underlyingQueue.TryDequeue(temp)
                    underlyingList.Add(temp)
                End While
                requiresSync = False
            End SyncLock
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            SyncLock m_syncRoot
                UpdateLists()
                Return underlyingList.GetEnumerator()
            End SyncLock
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        Public Sub Add(ByVal item As T) Implements IList(Of T).Add
            If requiresSync Then
                SyncLock m_syncRoot
                    underlyingQueue.Enqueue(item)
                End SyncLock
            Else
                underlyingQueue.Enqueue(item)
            End If
            isDirty = True
        End Sub


        Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
            SyncLock m_syncRoot
                UpdateLists()
                underlyingList.RemoveAt(index)
            End SyncLock
        End Sub

        Default Property IList_Item(ByVal index As Integer) As T Implements IList(Of T).Item
            Get
                SyncLock m_syncRoot
                    UpdateLists()
                    Return underlyingList(index)
                End SyncLock
            End Get
            Set(ByVal value As T)
                SyncLock m_syncRoot
                    UpdateLists()
                    underlyingList(index) = value
                End SyncLock
            End Set
        End Property

        Public ReadOnly Property IsReadOnly() As Boolean Implements IList(Of T).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Sub Clear() Implements IList(Of T).Clear
            SyncLock m_syncRoot
                UpdateLists()
                underlyingList.Clear()
            End SyncLock
        End Sub

        Public Function Contains(ByVal item As T) As Boolean Implements IList(Of T).Contains
            SyncLock m_syncRoot
                UpdateLists()
                Return underlyingList.Contains(item)
            End SyncLock
        End Function

        Public Sub CopyTo(ByVal array As T(), ByVal arrayIndex As Integer) Implements IList(Of T).CopyTo
            SyncLock m_syncRoot
                UpdateLists()
                underlyingList.CopyTo(array, arrayIndex)
            End SyncLock
        End Sub

        Public Function Remove(ByVal item As T) As Boolean Implements IList(Of T).Remove
            SyncLock m_syncRoot
                UpdateLists()
                Return underlyingList.Remove(item)
            End SyncLock
        End Function


        Public ReadOnly Property Count() As Integer Implements IList(Of T).Count
            Get
                SyncLock m_syncRoot
                    UpdateLists()
                    Return underlyingList.Count
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property SyncRoot() As Object
            Get
                Return m_syncRoot
            End Get
        End Property

        Public ReadOnly Property IsSynchronized() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
            SyncLock m_syncRoot
                UpdateLists()
                Return underlyingList.IndexOf(item)
            End SyncLock
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
            SyncLock m_syncRoot
                UpdateLists()
                underlyingList.Insert(index, item)
            End SyncLock
        End Sub

    End Class
End Namespace