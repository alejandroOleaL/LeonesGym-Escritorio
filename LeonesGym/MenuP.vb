Public Class MenuP
    Private Sub lblHora_Click(sender As Object, e As EventArgs) Handles lblHora.Click

    End Sub

    Private Sub horaFecha_Tick(sender As Object, e As EventArgs) Handles horaFecha.Tick
        lblHora.Text = DateTime.Now.ToLongTimeString()
        lblFecha.Text = DateTime.Now.ToLongDateString()
    End Sub

    Private Sub MenuP_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub IconButton2_Click(sender As Object, e As EventArgs) Handles IconButton2.Click
        Busqueda.Show()

    End Sub
End Class