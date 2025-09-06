Imports System
Imports System.ComponentModel.Design
Imports System.Diagnostics
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices

Namespace My
	' Token: 0x02000009 RID: 9
	<CompilerGenerated()>
	<DebuggerNonUserCode()>
	<HideModuleName()>
	Friend Module MySettingsProperty
		' Token: 0x17000009 RID: 9
		' (get) Token: 0x0600001C RID: 28 RVA: 0x000022D9 File Offset: 0x000004D9
		<HelpKeyword("My.Settings")>
		Friend ReadOnly Property Settings As MySettings
			Get
				Return MySettings.[Default]
			End Get
		End Property
	End Module
End Namespace
