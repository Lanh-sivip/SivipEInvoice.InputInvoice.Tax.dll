Imports System
Imports System.Runtime.CompilerServices


Public Class Parameter
	Public Property XMLBytes As Byte()

	Public Property XMLString As String
	Public Property JsonString As String
	Public Property ClientCode As String

	Public Property UnitCode As String

	Public Property QRCode As Byte()

	Public Property PDFFileName As String

	Public Property MSTKH As String

	Public Property ErrorLog As String

	Public Sub New()
		Me.XMLBytes = Nothing
		Me.QRCode = Nothing
		Me.XMLString = ""
		Me.JsonString = ""
		Me.ClientCode = ""
		Me.UnitCode = ""
		Me.PDFFileName = ""
		Me.MSTKH = ""
		Me.ErrorLog = ""
	End Sub

	<CompilerGenerated()>
	Private byte_0 As Byte()

	<CompilerGenerated()>
	Private string_0 As String

	<CompilerGenerated()>
	Private string_1 As String

	<CompilerGenerated()>
	Private string_2 As String

	<CompilerGenerated()>
	Private byte_1 As Byte()

	<CompilerGenerated()>
	Private string_3 As String

	<CompilerGenerated()>
	Private string_4 As String

	<CompilerGenerated()>
	Private string_5 As String
End Class
