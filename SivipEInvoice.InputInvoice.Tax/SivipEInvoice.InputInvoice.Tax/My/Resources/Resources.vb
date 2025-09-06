Imports System
Imports System.CodeDom.Compiler
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Resources
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices

Namespace My.Resources
	' Token: 0x02000007 RID: 7
	<CompilerGenerated()>
	<HideModuleName()>
	<DebuggerNonUserCode()>
	<GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")>
	Friend Module Resources
		' Token: 0x17000005 RID: 5
		' (get) Token: 0x06000015 RID: 21 RVA: 0x00002BCC File Offset: 0x00000DCC
		<EditorBrowsable(EditorBrowsableState.Advanced)>
		Friend ReadOnly Property ResourceManager As ResourceManager
			Get
				If Object.ReferenceEquals(Resources.resourceManager_0, Nothing) Then
					Resources.resourceManager_0 = New ResourceManager("SivipEInvoice.InputInvoice.Tax.Resources", GetType(Resources).Assembly)
				End If
				Return Resources.resourceManager_0
			End Get
		End Property

		' Token: 0x17000006 RID: 6
		' (get) Token: 0x06000016 RID: 22 RVA: 0x000022B1 File Offset: 0x000004B1
		' (set) Token: 0x06000017 RID: 23 RVA: 0x000022B8 File Offset: 0x000004B8
		<EditorBrowsable(EditorBrowsableState.Advanced)>
		Friend Property Culture As CultureInfo
			Get
				Return Resources.cultureInfo_0
			End Get
			Set(value As CultureInfo)
				Resources.cultureInfo_0 = value
			End Set
		End Property

		' Token: 0x04000008 RID: 8
		Private resourceManager_0 As ResourceManager

		' Token: 0x04000009 RID: 9
		Private cultureInfo_0 As CultureInfo
	End Module
End Namespace
