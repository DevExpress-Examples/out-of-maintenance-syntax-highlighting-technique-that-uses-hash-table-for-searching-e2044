Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports DevExpress.XtraRichEdit
Imports System.Collections.Generic
Imports DevExpress.XtraRichEdit.API.Native
Imports DevExpress.XtraRichEdit.Services.Implementation

Namespace SyntaxHighlighting
	Public Partial Class Form1
		Inherits Form
		Public Sub New()
			InitializeComponent()
			richEditControl1.LoadDocument("test.sql", DocumentFormat.PlainText)

			Dim service As ISyntaxHighlightService = richEditControl1.GetService(Of ISyntaxHighlightService)()
			Dim wrapper As MySyntaxHighlightServiceWrapper = New MySyntaxHighlightServiceWrapper(richEditControl1)
			richEditControl1.RemoveService(GetType(ISyntaxHighlightService))
			richEditControl1.AddService(GetType(ISyntaxHighlightService), wrapper)
		End Sub

		Private Sub richEditControl1_DocumentLoaded(ByVal sender As Object, ByVal e As EventArgs) Handles richEditControl1.DocumentLoaded
			ClearSyntaxHighlightServiceCache()
		End Sub

		Private Sub richEditControl1_EmptyDocumentCreated(ByVal sender As Object, ByVal e As EventArgs) Handles richEditControl1.EmptyDocumentCreated
			ClearSyntaxHighlightServiceCache()
		End Sub
		Private Sub ClearSyntaxHighlightServiceCache()
			Dim service As ISyntaxHighlightService = richEditControl1.GetService(Of ISyntaxHighlightService)()
			If service Is Nothing Then
				Return
			End If

			Dim wrapper As MySyntaxHighlightServiceWrapper = TryCast(service, MySyntaxHighlightServiceWrapper)
			If wrapper Is Nothing Then
				Return
			End If

			wrapper.ResetCache()
		End Sub

		Private Class MySyntaxHighlightServiceWrapper
			Implements ISyntaxHighlightService
			Private control As RichEditControl
			Private Shared str As String()
			Private paragraphHashes As List(Of Integer)
			Shared Sub New()
				str = New String() { "INSERT", "SELECT", "CREATE", "TABLE", "USE", "IDENTITY", "ON", "OFF", "NOT", "NULL", "WITH", "SET" }
				Array.Sort(str)
			End Sub
			Public Sub New(ByVal control As RichEditControl)
				Me.control = control
				paragraphHashes = New List(Of Integer)()

			End Sub
			Public Sub ResetCache()
				paragraphHashes.Clear()
			End Sub
			#Region "ISyntaxHighlightService Members"

			Public Sub Execute() Implements ISyntaxHighlightService.Execute
				Dim doc As Document = Me.control.Document
				Dim paragraphCount As Integer = doc.Paragraphs.Count
				Dim i As Integer = 0
				Do While i < paragraphCount
					HighlightParagraph(i)

					i += 1
				Loop
			End Sub

			Private Sub HighlightParagraph(ByVal paragraphIndex As Integer)
				Dim doc As Document = Me.control.Document
				Dim paragraph As Paragraph = doc.Paragraphs(paragraphIndex)
				Dim paragraphRange As DocumentRange = paragraph.Range
				Dim paragraphStart As Integer = paragraphRange.Start.ToInt()
				Dim text As String = doc.GetText(paragraphRange)
				Dim hash As Integer = text.GetHashCode()
				If paragraphIndex < paragraphHashes.Count AndAlso paragraphHashes(paragraphIndex) = hash Then
					Return
				End If
				Dim length As Integer = text.Length
				Dim prevWhiteSpaceIndex As Integer = -1
				Dim i As Integer = 0
				Do While i <= length
					Dim ch As Char

					If i < length Then
						ch = text.Chars(i)
					Else
						ch = " "c
					End If

					If Char.IsWhiteSpace(ch) OrElse Char.IsPunctuation(ch) Then
						Dim wordLength As Integer = i - prevWhiteSpaceIndex - 1
						If wordLength > 0 Then
							Dim wordStart As Integer = prevWhiteSpaceIndex + 1
							Dim word As String = text.Substring(wordStart, wordLength)
							Dim index As Integer = Array.BinarySearch(str, word)
							Dim range As DocumentRange = doc.CreateRange(paragraphStart + wordStart, wordLength)
							Dim cp As CharacterProperties = doc.BeginUpdateCharacters(range)
							If index >= 0 Then
								cp.ForeColor = Color.Blue
							Else
								cp.ForeColor = Color.Black
							End If
							doc.EndUpdateCharacters(cp)
						End If
						prevWhiteSpaceIndex = i

					End If
					i += 1
				Loop
				i = paragraphHashes.Count
				Do While i <= paragraphIndex
					paragraphHashes.Add(String.Empty.GetHashCode())
					i += 1
				Loop
				paragraphHashes(paragraphIndex) = hash
			End Sub
			#End Region
		End Class
	End Class
End Namespace