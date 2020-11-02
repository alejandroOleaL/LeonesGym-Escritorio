Imports System.IO
Imports System.Text
Imports DPFP
Imports DPFP.Capture
Imports MySql.Data.MySqlClient

Public Class Form1
    Implements DPFP.Capture.EventHandler

    Private Captura As DPFP.Capture.Capture
    Private Enroller As DPFP.Processing.Enrollment
    Private Delegate Sub _delegadoMuestra(ByVal text As String)
    Private Delegate Sub _delegadoControles()
    Private template As DPFP.Template

    Private Sub mostrarVeces(ByVal texto As String)
        If (vecesDedo.InvokeRequired) Then
            Dim deleg As New _delegadoMuestra(AddressOf mostrarVeces)
            Me.Invoke(deleg, New Object() {texto})
        Else
            vecesDedo.Text = texto
        End If
    End Sub

    Private Sub modificarControles()
        If (btnGuardar.InvokeRequired) Then
            Dim deleg As New _delegadoControles(AddressOf modificarControles)
            Me.Invoke(deleg, New Object() {})
        Else
            btnGuardar.Enabled = True
            txtNombre.Enabled = True
        End If
    End Sub

    Protected Overridable Sub Init()
        Try
            Captura = New Capture()

            If Not Captura Is Nothing Then
                Captura.EventHandler = Me
                Enroller = New DPFP.Processing.Enrollment()
                Dim text As New StringBuilder()
                text.AppendFormat("Necesitas pasar el dedo {0} veces", Enroller.FeaturesNeeded)
                vecesDedo.Text = text.ToString

            Else
                MessageBox.Show("No se pudo instancia la captura")
            End If
        Catch ex As Exception
            MessageBox.Show("No se pudo iniciar la captura")
        End Try
    End Sub

    Protected Sub iniciarCaptura()
        If Not Captura Is Nothing Then
            Try
                Captura.StartCapture()
            Catch ex As Exception
                MessageBox.Show("No se pudo iniciar la captura")
            End Try
        End If
    End Sub

    Protected Sub pararCaptura()
        If Not Captura Is Nothing Then
            Try
                Captura.StopCapture()
            Catch ex As Exception
                MessageBox.Show("No se pudo detener la captura")
            End Try
        End If
    End Sub
    Public Sub OnComplete(Capture As Object, ReaderSerialNumber As String, Sample As Sample) Implements EventHandler.OnComplete
        ponerImagen(ConvertirSampleMapadeBits(Sample))
        Procesar(Sample)
    End Sub

    Public Sub OnFingerGone(Capture As Object, ReaderSerialNumber As String) Implements EventHandler.OnFingerGone

    End Sub

    Public Sub OnFingerTouch(Capture As Object, ReaderSerialNumber As String) Implements EventHandler.OnFingerTouch
        'Se ejecuta al momento de tocar, podria ser para la funcion de tocar sin necesidad de algun boton
    End Sub

    Public Sub OnReaderConnect(Capture As Object, ReaderSerialNumber As String) Implements EventHandler.OnReaderConnect

    End Sub

    Public Sub OnReaderDisconnect(Capture As Object, ReaderSerialNumber As String) Implements EventHandler.OnReaderDisconnect

    End Sub

    Public Sub OnSampleQuality(Capture As Object, ReaderSerialNumber As String, CaptureFeedback As CaptureFeedback) Implements EventHandler.OnSampleQuality

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs)
        Init()
        iniciarCaptura()
    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        pararCaptura()
    End Sub

    Protected Function ConvertirSampleMapadeBits(ByVal Sample As DPFP.Sample) As Bitmap
        Dim convertidor As New DPFP.Capture.SampleConversion() 'es una variable de tipo conversor de un DPFP.Sample
        Dim mapaBits As Bitmap = Nothing
        convertidor.ConvertToPicture(Sample, mapaBits)
        Return mapaBits
    End Function

    Private Sub ponerImagen(ByVal bmp)
        imagenHuella.Image = bmp
    End Sub

    Protected Function extraerCaracteristicas(ByVal Sample As DPFP.Sample, ByVal Purpose As DPFP.Processing.DataPurpose) As DPFP.FeatureSet
        Dim extractor As New DPFP.Processing.FeatureExtraction()
        Dim alimentacion As DPFP.Capture.CaptureFeedback = DPFP.Capture.CaptureFeedback.None
        Dim caracteristicas As New DPFP.FeatureSet()
        extractor.CreateFeatureSet(Sample, Purpose, alimentacion, caracteristicas)
        If (alimentacion = DPFP.Capture.CaptureFeedback.Good) Then
            Return caracteristicas
        Else
            Return Nothing
        End If
    End Function

    Protected Sub Procesar(ByVal Sample As DPFP.Sample)
        Dim caracteristicas As DPFP.FeatureSet = extraerCaracteristicas(Sample, DPFP.Processing.DataPurpose.Enrollment)
        If (Not caracteristicas Is Nothing) Then
            Try
                Enroller.AddFeatures(caracteristicas)
            Finally
                Dim text As New StringBuilder()
                text.AppendFormat("Necesitas pasar el dedo {0} veces", Enroller.FeaturesNeeded)
                mostrarVeces(text.ToString())
                Select Case Enroller.TemplateStatus
                    Case DPFP.Processing.Enrollment.Status.Ready
                        template = Enroller.Template
                        pararCaptura()
                        modificarControles()
                    Case DPFP.Processing.Enrollment.Status.Failed
                        Enroller.Clear()
                        pararCaptura()
                        iniciarCaptura()
                End Select
            End Try
        End If
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        Dim builderconex As New MySqlConnectionStringBuilder()
        builderconex.Server = "localhost"
        builderconex.UserID = "root"
        builderconex.Password = ""
        builderconex.Database = "leones_bd"
        Dim conexion As New MySqlConnection(builderconex.ToString())
        conexion.Open()
        Dim cmd As New MySqlCommand()
        cmd = conexion.CreateCommand()
        If (txtNombre.Text.ToString().Equals("")) Then
            MessageBox.Show("Debes llenar el campo nombre")
        Else
            cmd.CommandText = "INSERT INTO usuarios(nombre, huella) VALUES(?,?)"
            cmd.Parameters.AddWithValue("Nombre", txtNombre.Text.ToString())
            Using fm As New MemoryStream(template.Bytes)
                cmd.Parameters.AddWithValue("huella", fm.ToArray())
            End Using
            cmd.ExecuteNonQuery()
            cmd.Dispose()
            conexion.Close()
            conexion.Dispose()
            MessageBox.Show("OK")
            btnGuardar.Enabled = False
            txtNombre.Enabled = False
        End If
    End Sub

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        pararCaptura()
        Dim ventanaBuscar As New Busqueda()
        ventanaBuscar.ShowDialog()
    End Sub

    Private Sub Form1_Leave(sender As Object, e As EventArgs) Handles MyBase.Leave
        pararCaptura()
    End Sub

    Private Sub Form1_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        Init()
        iniciarCaptura()
    End Sub
End Class
