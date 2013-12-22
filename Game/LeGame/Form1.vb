Public Class Form1

    Private Sub OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK.Click
        If UsernameTextBox.Text = "Invic" And TextBox1.Text = "pod-245-321-541" Then
            MsgBox("Podemu est activé pour pouvez l'excuter avec la même clé et nom")
            Me.Close()
        End If
        If UsernameTextBox.Text < "Invic" And TextBox1.Text > "pod-245-321-541" Then
            MsgBox("clé ou nom invalide")
        End If
    End Sub

    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel.Click
        Me.Close()
    End Sub

    Private Sub UsernameLabel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UsernameLabel.Click

    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub
End Class
