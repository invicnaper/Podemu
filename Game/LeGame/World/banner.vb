Imports System.IO
Public Class banner
    Private Sub ecritureFichier()

        'Instanciation du StreamWriter avec passage du nom du fichier 
        Dim monStreamWriter As StreamWriter = New StreamWriter("mj/banner.txt")

        'Ecriture du texte dans votre fichier
        monStreamWriter.WriteLine("Listes des joueurs banni et kicker ")
        monStreamWriter.WriteLine("Listes des joueurs banni et kicker ")

        'Fermeture du StreamWriter (Trés important)
        monStreamWriter.Close()

    End Sub
End Class
