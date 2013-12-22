Namespace World
    Public Class LoginKeys

        Public Shared KeyList As New List(Of Realm.LoginKey)

        Public Shared Function KeyExist(ByVal AccountName As String)

            For Each Key As Realm.LoginKey In KeyList.ToArray
                If Key.AccountName = AccountName Then Return True
            Next

            Return False
        End Function

        Public Shared Function RefreshKey(ByVal AccountName As String, ByVal NewSqlInfos As Realm.LoginInfos) As Realm.LoginKey

            For Each ActualGameKey As Realm.LoginKey In KeyList.ToArray
                If (ActualGameKey.AccountName = AccountName) Then
                    ActualGameKey.SqlInfos = NewSqlInfos
                    Return ActualGameKey
                End If
            Next

            Return Nothing

        End Function


        Public Shared Function GetKey(ByVal AccountName As String) As Realm.LoginKey

            For Each ActualGameKey As Realm.LoginKey In KeyList.ToArray
                If (ActualGameKey.AccountName = AccountName) Then
                    Return ActualGameKey
                End If
            Next

            Return Nothing

        End Function

    End Class
End Namespace