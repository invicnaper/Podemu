Imports System.Text
Imports System.IO
Imports MySql.Data.MySqlClient

Namespace Utils
    Public Class ttt




        Public Shared Sub Read(ByVal name As String)

            Dim InsertSub As String = "INSERT INTO subareas(id,name) VALUES(@id,@name)"
            Dim InsertSubCommand As New MySqlCommand(InsertSub, Sql.Others)

            Dim SetSub As String = "UPDATE maps_data SET Subarea=@Subarea,PosX=@x,PosY=@y WHERE id=@id"
            Dim SetSubCommand As New MySqlCommand(SetSub, Sql.Others)

            Dim reader As New StreamReader(name, UTF8Encoding.UTF8)

            While Not reader.EndOfStream

                Dim line = reader.ReadLine()
                Dim values = GetValues(line)

                If values.Count = 0 Then Continue While

                If line.Contains("MA.sa") Then

                    InsertSubCommand.Parameters.Clear()

                    InsertSubCommand.Parameters.Add(New MySqlParameter("@id", values("id")))
                    InsertSubCommand.Parameters.Add(New MySqlParameter("@name", values("n").Replace("/", "").Replace("\", "").Replace(Chr(34), "")))

                    InsertSubCommand.ExecuteNonQuery()

                ElseIf line.Contains("MA.m") Then

                    'SetSubCommand.Parameters.Clear()

                    'SetSubCommand.Parameters.Add(New MySqlParameter("@id", values("id")))
                    'SetSubCommand.Parameters.Add(New MySqlParameter("@Subarea", values("sa")))
                    'SetSubCommand.Parameters.Add(New MySqlParameter("@x", values("x")))
                    'SetSubCommand.Parameters.Add(New MySqlParameter("@y", values("y")))

                    'SetSubCommand.ExecuteNonQuery()

                End If


            End While


        End Sub

        Public Shared Function GetValues(ByVal data As String) As Dictionary(Of String, String)
            Dim v As New Dictionary(Of String, String)
            If data.Contains("{") AndAlso data.Contains("}") Then

                Dim f0 = data.Split("=")(0)

                v.Add("id", f0.Substring(data.IndexOf("[") + 1, data.IndexOf("]") - data.IndexOf("[")).Trim())

                Dim d = data.Substring(data.IndexOf("{") + 1, data.IndexOf("}") - data.IndexOf("{"))
                Dim e = d.Split(",")

                For Each value In e
                    Try
                        v.Add(value.Split(":")(0).Trim(), value.Split(":")(1).Trim())
                    Catch ex As Exception
                    End Try
                Next

            End If
            Return v
        End Function


    End Class
End Namespace