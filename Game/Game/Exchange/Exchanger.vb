Namespace Game
    Public Class Exchanger

        Public Exchange As Exchange

        Public Client As GameClient
        Public HasValidate As Boolean
        Public Kamas As Long
        Public Items As New List(Of ExchangeItem)


        Public Sub New(ByVal Client As GameClient, ByVal Exchange As Exchange)
            Me.Client = Client
            Me.Exchange = Exchange

            Client.Character.State.BeginTrade(Trading.Exchange, Exchange)

        End Sub

        Public Sub Begin()
            Client.Send("ECK1")
        End Sub

        Public Sub Leave()

            Client.Character.State.EndTrade()

            Client.Send("EV")

        End Sub

        Public Sub Finish()
            Client.Send("EVa")
        End Sub

        Public Sub Unvalidate()

            HasValidate = False
            Client.Send("EK0" & Client.Character.ID)

        End Sub

        Public Sub Unvalidate(ByVal Id As Integer)


            Client.Send("EK0" & Id)

        End Sub

        Public Sub Validate()
            Client.Send("EK1" & Client.Character.ID)
        End Sub

        Public Sub Validate(ByVal Id As Integer)
            Client.Send("EK1" & Id)
        End Sub

    End Class
End Namespace