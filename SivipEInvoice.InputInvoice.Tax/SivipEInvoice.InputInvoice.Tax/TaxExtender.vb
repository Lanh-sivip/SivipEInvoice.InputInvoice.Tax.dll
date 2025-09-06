Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Net.Security
Imports System.Runtime.CompilerServices
Imports System.Runtime.CompilerServices.RuntimeHelpers
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web
Imports System.Web.Configuration
Imports System.Web.Script.Services
Imports System.Web.Services
Imports System.Web.SessionState
Imports System.Xml
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CompilerServices
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports SivipEInvoice.GetInvoiceData



<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ScriptService()>
<WebService([Namespace]:="http://www.sivip.vn/")>
Public Class TaxExtender
    Inherits WebService
    Implements IRequiresSessionState

    Public Sub New()
        Me.sysDatabaseName = ""
        Me.appDatabaseName = ""
        Me.sysConnectionString = ""
        Me.commandTimeout = 300
        Me.invoiceTimeout = 480000
        Me.networkKey = ""
        Me.tblTmp = "taxreceive"
        Me.invoiceService = 0
    End Sub

    Private Shared Function checkToken(object0 As Object, object1 As Object, object2 As Object, sslPolicyErrors0 As SslPolicyErrors) As Boolean
        Return True
    End Function

    <WebMethod(EnableSession:=True)>
    Public Function GetCaptcha() As Captcha
        Dim captchasvg As String = ""
        Dim ckey As String = ""
        Methods.GetTaxCaptcha(ckey, captchasvg, 1)
        Return New Captcha() With {.CKey = ckey, .CaptchaSVG = captchasvg}
    End Function

    <WebMethod(EnableSession:=True)>
    Public Function GetToken(userName As String, password As String, ckey As String, captcha As String) As String
        Dim client_code As String = Conversions.ToString(Sthink.AjaxControlExtender.Session.GetSession(7))
        Dim objectValue As Object = RuntimeHelpers.GetObjectValue(Sthink.AjaxControlExtender.Session.GetSession(4))
        userName = Me.ClearString(userName)
        Dim string_ As String = client_code
        Dim appConnectionString As String = ""
        Me.GetAppSetting(string_, Me.sysConnectionString, appConnectionString, Conversions.ToString(objectValue))
        Dim list As List(Of String) = Me.getInfoToken(client_code, userName, Conversions.ToString(objectValue))
        If String.IsNullOrEmpty(password) Then
            password = list(2)
        End If
        If list.Count > 3 Then
            client_code = list(3)
        End If
        password = Me.AESDecrypt(client_code.Trim(), password.Trim())
        ckey = Me.ClearString(ckey)
        captcha = Me.ClearString(captcha)
        Return Methods.GetTaxToken(userName, password, captcha, ckey, 1)
    End Function

    <WebMethod(EnableSession:=True)>
    Public Function CheckToken(userName As String) As Boolean
        Return Methods.CheckTaxToken(Me.getInfoToken(Conversions.ToString(Sthink.AjaxControlExtender.Session.GetSession(7)), Me.ClearString(userName), Conversions.ToString(Sthink.AjaxControlExtender.Session.GetSession(4)))(0))
    End Function

    <WebMethod(EnableSession:=True)>
    Public Function TaxImport(searchType As Integer, clientCode As String, userName As String, unitCode As String, userID As Integer, limitImportRow As Integer, dateFrom As String, dateTo As String, taxCode As String, form As String, serial As String, invoiceNumber As String, invoiceStatus As String, processStatus As String) As Result
        Return Me.Import(searchType, clientCode, userName, unitCode, userID, limitImportRow, dateFrom, dateTo, taxCode, form, serial, invoiceNumber, invoiceStatus, processStatus, 0, Conversions.ToString(Sthink.AjaxControlExtender.Session.GetSession(4)))
    End Function

    <WebMethod(EnableSession:=True)>
    Public Function Import(searchType As Integer, clientCode As String, userName As String, unitCode As String, userID As Integer, limitImportRow As Integer, dateFrom As String, dateTo As String, taxCode As String, form As String, serial As String, invoiceNumber As String, invoiceStatus As String, processStatus As String, schedule As Integer, appData As String) As Result
        clientCode = Me.ClearString(clientCode)
        Dim appConnectionString As String = ""
        Me.GetAppSetting(clientCode, Me.sysConnectionString, appConnectionString, appData)
        userName = Me.ClearString(userName)
        unitCode = Me.ClearString(unitCode)
        dateFrom = Me.ClearString(dateFrom)
        dateTo = Me.ClearString(dateTo)
        taxCode = Me.ClearString(taxCode)
        form = Me.ClearString(form)
        serial = Me.ClearString(serial)
        invoiceNumber = Me.ClearString(invoiceNumber)
        invoiceStatus = Me.ClearString(invoiceStatus)
        processStatus = Me.ClearString(processStatus)
        appData = Me.ClearString(appData)
        If clientCode.Length > 6 Then
            clientCode = AESDecrypt2(clientCode, Me.networkKey)
            userName = AESDecrypt2(userName, Me.networkKey)
            appData = AESDecrypt2(appData, Me.networkKey)
        End If
        Dim list As List(Of String) = Me.getInfoToken(clientCode, userName, appData)
        Dim token As String = list(0)
        Dim tax_code As String = list(1)
        Dim result As Result
        If Not Methods.CheckTaxToken(token) Then
            result = New Result() With {.Total = 0, .TotalExist = 0, .TotalExcept = 0, .TotalError = 0, .Import = 0, .Successed = False, .Message = "001"}
        Else
            Me.DecryptPram(unitCode, dateFrom, dateTo, taxCode, form, serial, invoiceNumber, invoiceStatus, processStatus)
            invoiceStatus = Conversions.ToString(Interaction.IIf(Operators.CompareString(invoiceStatus, "*", False) = 0, "", invoiceStatus))
            processStatus = Conversions.ToString(Interaction.IIf(Operators.CompareString(processStatus, "*", False) = 0, "", processStatus))
            dateFrom = Strings.Format(DateTime.Parse(dateFrom), "yyyy-MM-dd")
            dateTo = Strings.Format(DateTime.Parse(dateTo), "yyyy-MM-dd")
            If list.Count > 3 Then
                clientCode = list(3)
            End If
            Dim dataTable As DataTable = New DataTable()
            Dim dataTable_ As DataTable = New DataTable()
            Dim dataTable_Pos As DataTable = New DataTable()
            Dim total As Integer = 0
            Dim total_Pos As Integer = 0
            Dim totalExist As Integer = 0
            Dim totalExcept As Integer = 0
            If Me.getInvoiceList(searchType, dataTable_, total, token, dateFrom, dateTo, taxCode, form, serial, invoiceNumber, invoiceStatus, processStatus) Then
                If String.IsNullOrEmpty(processStatus) Then
                    If Not Me.getInvoiceList(searchType, dataTable_Pos, total_Pos, token, dateFrom, dateTo, taxCode, form, serial, invoiceNumber, invoiceStatus, "8") Then
                        result = New Result() With {.Total = 0, .TotalExist = 0, .TotalExcept = 0, .TotalError = 0, .Import = 0, .Successed = False, .Message = "002"}
                        Return result
                    End If
                End If
                Dim dtresult As DataTable = dataTable_.Copy()
                For Each row As DataRow In dataTable_Pos.Rows
                    dtresult.ImportRow(row)
                Next
                If Not Me.UpdateInputInvoice(searchType, dataTable, dtresult, totalExist, totalExcept, clientCode, tax_code, userID, limitImportRow, dateFrom, dateTo, processStatus, appConnectionString, schedule) Then
                    result = New Result() With {.Total = 0, .TotalExist = 0, .TotalExcept = 0, .TotalError = 0, .Import = 0, .Successed = False, .Message = "003"}
                ElseIf dataTable IsNot Nothing AndAlso dataTable.Rows IsNot Nothing AndAlso dataTable.Rows.Count <> 0 Then
                    If Not Me.getInvoiceDetail(searchType, dataTable, clientCode, tax_code, unitCode, userID, token, taxCode, form, serial, invoiceNumber, dateFrom, dateTo, processStatus, appConnectionString, schedule) Then
                        result = New Result() With {.Total = 0, .TotalExist = 0, .TotalExcept = 0, .TotalError = 0, .Import = 0, .Successed = False, .Message = "004"}
                    Else
                        Dim totalError As Integer = Me.UpdateInvoiceTaxError(clientCode, tax_code, userID, dateFrom, dateTo, taxCode, form, serial, invoiceNumber, processStatus, appConnectionString)
                        result = New Result() With {.Total = total + total_Pos, .TotalExist = totalExist, .TotalExcept = totalExcept, .TotalError = totalError, .Import = 0, .Successed = True, .Message = "OK"}
                        If searchType = "1" Then
                            Dim thucthi As String = sivip.sivip.tcthddr(dateFrom, dateTo)
                        Else
                            Dim thucthi As String = sivip.sivip.tcthddv(dateFrom, dateTo)
                        End If
                    End If
                Else
                    Dim totalError As Integer = Me.UpdateInvoiceTaxError(clientCode, tax_code, userID, dateFrom, dateTo, taxCode, form, serial, invoiceNumber, processStatus, appConnectionString)
                    result = New Result() With {.Total = total + total_Pos, .TotalExist = totalExist, .TotalExcept = totalExcept, .TotalError = totalError, .Import = 0, .Successed = True, .Message = "OKKO"}
                End If
            Else
                result = New Result() With {.Total = 0, .TotalExist = 0, .TotalExcept = 0, .TotalError = 0, .Import = 0, .Successed = False, .Message = "002"}
            End If
        End If
        Return result
    End Function

    Private Function getInvoiceDetail(searchType As Integer, dataTable_0 As DataTable, clientCode As String, tax_code As String, unitCode As String, userID As Integer, token As String, taxCode As String, form As String, serial As String, invoiceNumber As String, dateFrom As String, dateTo As String, processStatus As String, appConnectionString As String, Optional schedule As Integer = 0) As Boolean
        Dim result As Boolean
        Try
            Dim list As List(Of Parameter) = New List(Of Parameter)()
            If dataTable_0 IsNot Nothing AndAlso dataTable_0.Rows IsNot Nothing AndAlso dataTable_0.Rows.Count > 0 Then
                Dim count As Integer = dataTable_0.Rows.Count
                If schedule = 1 Then
                    Me.Retrieve(String.Format("exec dbo.Sthink$InputInvoiceTax$Save '{0}', '{1}', '{2}'", clientCode, tax_code, Strings.Format(DateTime.Parse(Conversions.ToString(dataTable_0.Rows(count - 1)("ngay_hd"))), "yyyyMMdd")), False, "")
                End If
                Dim num As Integer = count - 1
                For i As Integer = 0 To num
                    Dim dataRow As DataRow = dataTable_0.Rows(i)
                    Dim mst As String = dataRow("mst").ToString().Trim()
                    Dim mau_so As String = dataRow("mau_so").ToString().Trim()
                    Dim ky_hieu As String = dataRow("ky_hieu").ToString().Trim()
                    Dim so_hd As Integer = Integer.Parse(dataRow("so_hd").ToString().Trim())
                    Dim ngay_hd As String = Strings.Format(DateTime.Parse(dataRow("ngay_hd").ToString()), "yyyyMMdd")
                    Dim num2 As Integer = Integer.Parse(dataRow("error_count").ToString().Trim())
                    Dim processStatusDB As String = dataRow("type").ToString().Trim()
                    Dim invoiceJson As String
                    Dim messageError As String = ""
                    If Operators.CompareString(processStatusDB, "8", False) <> 0 Then
                        invoiceJson = Methods.GetTaxInvoiceJson(searchType, token, mst, mau_so, ky_hieu, Conversions.ToString(so_hd), 1, messageError)
                    Else
                        invoiceJson = Methods.GetTaxInvoiceJsonPOS(searchType, token, mst, mau_so, ky_hieu, Conversions.ToString(so_hd), 1, messageError)
                    End If
                    If Operators.CompareString(invoiceJson, "999", False) <> 0 Then
                        Dim parameter As Parameter
                        parameter = Me.getMasterTaxInvoice(invoiceJson, processStatusDB)
                        If Operators.CompareString(parameter.ErrorLog, "", False) = 0 Then
                            If num2 > 0 Then
                                Me.InvoiceTaxUpdateError(clientCode, appConnectionString, tax_code, mst, mau_so, ky_hieu, Conversions.ToString(so_hd), ngay_hd, userID, 1, "")
                            End If
                            parameter.ClientCode = clientCode
                            parameter.UnitCode = unitCode
                            list.Add(parameter)
                        Else
                            Me.InvoiceTaxUpdateError(clientCode, appConnectionString, tax_code, mst, mau_so, ky_hieu, Conversions.ToString(so_hd), ngay_hd, userID, 0, parameter.ErrorLog)
                        End If
                    Else
                        Me.InvoiceTaxUpdateError(clientCode, appConnectionString, tax_code, mst, mau_so, ky_hieu, Conversions.ToString(so_hd), ngay_hd, userID, 0, messageError)
                    End If
                Next
            End If
            Me.getDetailTaxInvoice(searchType, list, userID, Me.tblTmp, dateFrom, dateTo, appConnectionString)
            result = True
        Catch ex3 As Exception
            Me.WriteLogEntry("GetInputInvoice: " + ex3.Message + ex3.StackTrace)
            result = False
        End Try
        Return result
    End Function

    Private Function getInvoiceList(searchType As Integer, ByRef dataTable_ As DataTable, ByRef total As Integer, token As String, dateFrom As String, dateTo As String, taxCode As String, form As String, serial As String, invoiceNumber As String, invoiceStatus As String, processStatus As String) As Boolean
        Dim result As Boolean
        Try
            Dim text As String = ""
            Me.AddColumns(dataTable_, GetType(String), New String() {"mst", "mau_so", "ky_hieu", "so_hd", "tt_hd", "tt_xl", "id"})
            Me.AddColumns(dataTable_, GetType(DateTime), New String() {"ngay_hd"})

            Dim startDate As DateTime = Conversions.ToDate(dateFrom)
            Dim endDate As DateTime = Conversions.ToDate(dateTo)
            Dim currentMonth As DateTime = startDate

            While currentMonth <= endDate
                'Set date from and date to
                Dim firstDayOfPeriod As DateTime
                If currentMonth.Month = startDate.Month And currentMonth.Year = startDate.Year Then
                    firstDayOfPeriod = startDate
                Else
                    firstDayOfPeriod = New DateTime(currentMonth.Year, currentMonth.Month, 1)
                End If

                Dim lastDayOfPeriod As DateTime
                If currentMonth.Month = endDate.Month And currentMonth.Year = endDate.Year Then
                    lastDayOfPeriod = endDate
                Else
                    lastDayOfPeriod = New DateTime(currentMonth.Year, currentMonth.Month, 1).AddMonths(1).AddDays(-1)
                End If
                'Process
                Dim totalMonth As Integer = 0
                While True
                    Dim text2 As String
                    If Operators.CompareString(processStatus, "8", False) = 0 Then
                        text2 = Methods.GetTaxInvoiceListPOS(searchType, token, firstDayOfPeriod, lastDayOfPeriod, taxCode, form, serial, invoiceNumber, invoiceStatus, processStatus, 1, text)
                    Else
                        text2 = Methods.GetTaxInvoiceList(searchType, token, firstDayOfPeriod, lastDayOfPeriod, taxCode, form, serial, invoiceNumber, invoiceStatus, processStatus, 1, text)
                    End If
                    If text2.Contains("datas") Then
                        Dim jobject As JObject = JsonConvert.DeserializeObject(Of JObject)(text2, Methods.GetJsonSetting())
                        Dim jtoken As JToken = jobject.SelectToken("datas")
                        If Not totalMonth <> 0 Then
                            totalMonth = CInt(Conversions.ToLong(jobject.SelectToken("total").ToString()))
                        End If
                        Dim dataRow As DataRow = dataTable_.NewRow()
                        If jtoken.HasValues Then
                            Dim num As Integer = jtoken.Count() - 1
                            For i As Integer = 0 To num
                                dataRow = dataTable_.NewRow()
                                Dim jtoken2 As JToken = jtoken(i)
                                Dim value As String = jtoken2.SelectToken("tdlap").ToString()
                                dataRow("mst") = jtoken2.SelectToken("nbmst").ToString()
                                dataRow("mau_so") = jtoken2.SelectToken("khmshdon").ToString()
                                dataRow("ky_hieu") = jtoken2.SelectToken("khhdon").ToString()
                                dataRow("so_hd") = jtoken2.SelectToken("shdon").ToString()
                                dataRow("tt_hd") = jtoken2.SelectToken("tthai").ToString()
                                dataRow("tt_xl") = jtoken2.SelectToken("ttxly").ToString()
                                dataRow("id") = jtoken2.SelectToken("id").ToString()
                                dataRow("ngay_hd") = RuntimeHelpers.GetObjectValue(If((Not String.IsNullOrEmpty(value)), Convert.ToDateTime(value), DBNull.Value))
                                dataTable_.Rows.Add(dataRow)
                                dataTable_.AcceptChanges()
                            Next
                        End If
                        Dim jtoken3 As JToken = jobject.SelectToken("state")
                        If Not String.IsNullOrEmpty(jtoken3.ToString()) Then
                            text = jtoken3.ToString()
                        Else
                            text = ""
                        End If
                        If String.IsNullOrEmpty(text) Then
                            result = True
                            Exit While
                        End If
                    ElseIf text2.Contains("message") Or text2.Contains("999") Then
                        result = False
                        Exit While
                    End If
                End While
                total += totalMonth
                If Not result Then
                    Exit While
                End If
                'Next month
                currentMonth = currentMonth.AddMonths(1)
            End While
        Catch ex As Exception
            Me.WriteLogEntry("GetDataFromTaxAuthority: " + ex.Message + ex.StackTrace)
            result = False
        End Try
        Return result
    End Function

    Private Function UpdateInputInvoice(searchType As Integer, ByRef dataTable_0 As DataTable, dataTable_1 As DataTable, ByRef totalExist As Integer, ByRef totalExcept As Integer, clientCode As String, tax_code As String, userID As Integer, limitImportRow As Integer, dateFrom As String, dateTo As String, processStatus As String, appConnectionString As String, Optional schedule As Integer = 0) As Boolean
        Dim result As Boolean
        Try
            If dataTable_1 IsNot Nothing AndAlso dataTable_1.Rows IsNot Nothing AndAlso dataTable_1.Rows.Count > 0 Then
                Dim dataSet As DataSet = New DataSet()
                Dim tblTmp As String = "taxjson"
                Dim commandTextCreate As String = String.Format("create table #{0} (mst varchar(32), mau_so varchar(32), ky_hieu varchar(32), so_hd varchar(32), tt_hd varchar(16), tt_xl varchar(16), id char(64), ngay_hd smalldatetime)", tblTmp)
                Dim commandTextExec As String = String.Format("exec Sthink$GetInputInvoice '#{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, {7}, {8}, {9}", New Object() {tblTmp, clientCode, tax_code, Strings.Format(DateTime.Parse(dateFrom), "yyyyMMdd"), Strings.Format(DateTime.Parse(dateTo), "yyyyMMdd"), processStatus, searchType, userID, limitImportRow, schedule})
                Try
                    dataSet = Me.BulkCopyData(appConnectionString, dataTable_1, tblTmp, commandTextCreate, commandTextExec)
                Catch ex As Exception
                    Return False
                End Try
                If dataSet Is Nothing OrElse dataSet.Tables Is Nothing OrElse dataSet.Tables.Count = 0 Then
                    Return False
                End If
                If dataSet.Tables.Count > 0 Then
                    dataTable_0 = dataSet.Tables(0)
                End If
                If dataSet.Tables.Count > 1 Then
                    Dim dataTable As DataTable = dataSet.Tables(1)
                    If dataTable IsNot Nothing AndAlso dataTable.Rows IsNot Nothing AndAlso dataTable.Rows.Count > 0 Then
                        totalExist = Conversions.ToInteger(dataTable.Rows(0)(0))
                    End If
                End If
                If dataSet.Tables.Count > 2 Then
                    Dim dataTable2 As DataTable = dataSet.Tables(2)
                    If dataTable2 IsNot Nothing AndAlso dataTable2.Rows IsNot Nothing AndAlso dataTable2.Rows.Count > 0 Then
                        totalExcept = Conversions.ToInteger(dataTable2.Rows(0)(0))
                    End If
                End If
            End If
            result = True
        Catch ex2 As Exception
            Me.WriteLogEntry("GetDataFromJson: " + ex2.Message + ex2.StackTrace)
            result = False
        End Try
        Return result
    End Function

    Private Function UpdateInvoiceTaxError(clientCode As String, tax_code As String, userID As Integer, dateFrom As String, dateTo As String, taxCode As String, form As String, serial As String, invoiceNumber As String, processStatus As String, appConnectionString As String) As Integer
        Dim commandText As String = String.Format("exec dbo.Sthink$InputInvoiceTax$ErrorCount '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', {9}", New Object() {clientCode, tax_code, Strings.Format(DateTime.Parse(dateFrom), "yyyyMMdd"), Strings.Format(DateTime.Parse(dateTo), "yyyyMMdd"), taxCode, form, serial, invoiceNumber, processStatus, userID.ToString()})
        Dim num As Integer = 0
        Try
            Dim dataSet As DataSet = Me.Retrieve(commandText, True, appConnectionString)
            If dataSet IsNot Nothing AndAlso dataSet.Tables.Count > 0 Then
                Dim num2 As Integer = dataSet.Tables.Count - 1
                For i As Integer = 0 To num2
                    Dim dataTable As DataTable = dataSet.Tables(i)
                    If dataTable IsNot Nothing AndAlso dataTable.Rows IsNot Nothing AndAlso dataTable.Rows.Count > 0 Then
                        Dim num3 As Integer = dataTable.Rows.Count - 1
                        For j As Integer = 0 To num3
                            num = Conversions.ToInteger(Operators.AddObject(num, dataTable.Rows(j)(0)))
                        Next
                    End If
                Next
            End If
        Catch ex As Exception
            Me.WriteLogEntry("GetTaxError: " + ex.Message + ex.StackTrace)
        End Try
        Return num
    End Function

    Private Function getInfoToken(clientCode As String, userName As String, appData As String) As List(Of String)
        Dim list As List(Of String) = New List(Of String)() From {"", "", ""}
        Try
            Dim appConnectionString As String = ""
            Me.GetAppSetting(clientCode, Me.sysConnectionString, appConnectionString, appData)
            Dim dataTable As DataTable = Me.Retrieve(String.Format("exec Sthink$InputInvoice$Tax$GetInfo '{0}', '{1}'", clientCode, userName))
            Dim dataRow As DataRow = dataTable.Rows(0)
            list(0) = dataRow("token").ToString()
            list(1) = dataRow("tax_code").ToString()
            list(2) = dataRow("password").ToString()
            If dataTable.Columns.Contains("client_code") Then
                list.Add(dataRow("client_code").ToString())
            End If
        Catch ex As Exception
        End Try
        Return list
    End Function

    Private Sub DecryptPram(ByRef unitCode As String, ByRef dateFrom As String, ByRef dateTo As String, ByRef taxCode As String, ByRef form As String, ByRef serial As String, ByRef invoiceNumber As String, ByRef invoiceStatus As String, ByRef processStatus As String)
        If Operators.CompareString(Me.networkKey, "", False) <> 0 And dateTo.Length > 10 Then
            unitCode = AESDecrypt2(unitCode, Me.networkKey)
            dateFrom = AESDecrypt2(dateFrom, Me.networkKey)
            dateTo = AESDecrypt2(dateTo, Me.networkKey)
            taxCode = AESDecrypt2(taxCode, Me.networkKey)
            form = AESDecrypt2(form, Me.networkKey)
            serial = AESDecrypt2(serial, Me.networkKey)
            invoiceNumber = AESDecrypt2(invoiceNumber, Me.networkKey)
            invoiceStatus = AESDecrypt2(invoiceStatus, Me.networkKey)
            processStatus = AESDecrypt2(processStatus, Me.networkKey)
        End If
    End Sub

    Private Sub AddColumns(ByRef _dataTable As DataTable, typeData As Type, ParamArray cols As String())
        For Each text As String In cols
            If Not _dataTable.Columns.Contains(text) Then
                _dataTable.Columns.Add(text, typeData)
            End If
        Next
    End Sub

    Private Function getMasterTaxInvoice(invoiceJson As String, processStatus As String) As Parameter
        Dim parameter As Parameter = New Parameter()
        Dim result As Parameter
        Try
            parameter.JsonString = invoiceJson
            If Not String.IsNullOrEmpty(invoiceJson) Then
                Dim jtoken As JToken = JsonConvert.DeserializeObject(Of JObject)(invoiceJson, Methods.GetJsonSetting())
                If jtoken.HasValues Then
                    If Me.checkMst(jtoken.SelectToken("nbmst").ToString()) Then
                        parameter.MSTKH = jtoken.SelectToken("nmmst")
                        result = parameter
                    Else
                        parameter.ErrorLog = "Error XML: MST NBan"
                        result = parameter
                    End If
                Else
                    parameter.ErrorLog = "Error XML: Template"
                    result = parameter
                End If
            Else
                parameter.ErrorLog = "Error XML: Template"
                result = parameter
            End If
        Catch ex As Exception
            Me.WriteLogEntry("GetAttached: " + ex.Message + ex.StackTrace)
            parameter.ErrorLog = "Error GetAttached: " + ex.Message + ex.StackTrace
            result = parameter
        End Try
        Return result
    End Function

    Private Sub getDetailTaxInvoice(searchType As Integer, listInvoice As List(Of Parameter), userID As Integer, tblTmp As String, dateFrom As String, dateTo As String, appConnectionString As String)
        If listInvoice IsNot Nothing AndAlso listInvoice.Count <> 0 Then
            Dim dataTable As DataTable = New DataTable()
            dataTable.Columns.Add("id")
            dataTable.Columns.Add("json")
            dataTable.Columns.Add("client_code")
            dataTable.Columns.Add("unit_code")
            dataTable.Columns.Add("use_id", GetType(Integer))
            dataTable.Columns.Add("ngay_ct", GetType(DateTime))
            dataTable.Columns.Add("search_type", GetType(Integer))
            Dim num As Integer = listInvoice.Count - 1
            For i As Integer = 0 To num
                Dim jsonString As String = listInvoice(i).JsonString
                Dim jtoken As JToken = JsonConvert.DeserializeObject(Of JObject)(jsonString, Methods.GetJsonSetting())
                Dim id As String = ""
                Dim nLap As String = ""
                Dim nmuaTen As String = ""
                Dim nmuaDchi As String = ""
                If jtoken.HasValues Then
                    id = jtoken.SelectToken("id").ToString()
                    nLap = jtoken.SelectToken("tdlap").ToString()
                    nmuaTen = jtoken.SelectToken("nmten").ToString()
                    nmuaDchi = jtoken.SelectToken("nmdchi").ToString()
                End If
                Dim dataRow As DataRow = dataTable.NewRow()
                dataRow(0) = id
                dataRow(1) = jsonString
                dataRow(2) = listInvoice(i).ClientCode
                dataRow(3) = listInvoice(i).UnitCode
                dataRow(4) = userID
                dataRow(5) = RuntimeHelpers.GetObjectValue(If(String.IsNullOrEmpty(nLap), DBNull.Value, Convert.ToDateTime(nLap)))
                dataRow(6) = searchType
                dataTable.Rows.Add(dataRow)
                dataTable.AcceptChanges()
            Next
            Try
                Dim commandTextCreate As String
                Dim commandTextExec As String
                commandTextCreate = String.Format("create table #{0} (id char(64), json ntext, client_code varchar(6), unit_code varchar(32), user_id int, ngay_ct smalldatetime, search_type int)", tblTmp)
                commandTextExec = String.Format("exec rs_AutoCreateListInputInvoiceSchedule '#{0}', '{1}', '{2}', 'json', 'id', 'unit_code', 'user_id', 'ngay_ct', 'search_type', '{3}'", tblTmp, Strings.Format(DateTime.Parse(dateFrom), "yyyyMMdd"), Strings.Format(DateTime.Parse(dateTo), "yyyyMMdd"), userID)
                Dim dataTable5 As DataTable = New DataTable()
                Try
                    dataTable5 = Me.BulkCopyData(appConnectionString, dataTable, tblTmp, commandTextCreate, commandTextExec).Tables(0)
                Catch ex2 As Exception
                End Try
                If dataTable5 IsNot Nothing AndAlso dataTable5.Rows IsNot Nothing Then
                    Dim count As Integer = dataTable5.Rows.Count
                End If
            Catch ex3 As Exception
                Me.WriteLogEntry("ImportDataToDB: " + ex3.Message + ex3.StackTrace)
            End Try
        End If
    End Sub

    Private Function checkMst(strMst As String) As Boolean
        If strMst IsNot Nothing Then
            Try
                Dim text As String = strMst.Replace("-", "")
                If (text.Length <> 10 AndAlso text.Length <> 12 AndAlso text.Length <> 13) OrElse Not Versioned.IsNumeric(text) Then
                    Return False
                End If
                Dim num As Integer = Integer.Parse(Conversions.ToString(text(0)))
                Dim num2 As Integer = Integer.Parse(Conversions.ToString(text(1)))
                Dim num3 As Integer = Integer.Parse(Conversions.ToString(text(2)))
                Dim num4 As Integer = Integer.Parse(Conversions.ToString(text(3)))
                Dim num5 As Integer = Integer.Parse(Conversions.ToString(text(4)))
                Dim num6 As Integer = Integer.Parse(Conversions.ToString(text(5)))
                Dim num7 As Integer = Integer.Parse(Conversions.ToString(text(6)))
                Dim num8 As Integer = Integer.Parse(Conversions.ToString(text(7)))
                Dim num9 As Integer = Integer.Parse(Conversions.ToString(text(8)))
                Dim num10 As Integer = Integer.Parse(Conversions.ToString(text(9)))
                If text.Length = 12 Then
                    Integer.Parse(Conversions.ToString(text(10)))
                    Integer.Parse(Conversions.ToString(text(11)))
                End If
                If text.Length = 13 Then
                    Integer.Parse(Conversions.ToString(text(10)))
                    Integer.Parse(Conversions.ToString(text(11)))
                    Integer.Parse(Conversions.ToString(text(12)))
                End If
                If text.Length <> 12 Then
                    Dim num11 As Integer = num * 31 + num2 * 29 + num3 * 23 + num4 * 19 + num5 * 17 + num6 * 13 + num7 * 7 + num8 * 5 + num9 * 3
                    If num10 <> 10 - num11 Mod 11 Then
                        Return False
                    End If
                End If
                Return True
            Catch ex As Exception
                Return False
            End Try
        End If
        Return False
    End Function

    Private Function AESDecrypt(ciphertext As String, key As String) As String
        Dim result As String
        Try
            If String.IsNullOrEmpty(ciphertext) Then
                ciphertext = "000000"
            End If
            Dim password As String = ciphertext
            Dim s As String = Strings.Left(ciphertext + ciphertext + ciphertext, 16)
            Dim aesManaged As AesManaged = New AesManaged()
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(s)
            Dim array As Byte() = Me.HexStringToByteArray(key)
            Dim rfc2898DeriveBytes As Rfc2898DeriveBytes = New Rfc2898DeriveBytes(password, bytes, 10)
            aesManaged.Key = rfc2898DeriveBytes.GetBytes(32)
            aesManaged.IV = bytes
            Dim cryptoTransform As ICryptoTransform = aesManaged.CreateDecryptor()
            result = Encoding.UTF8.GetString(cryptoTransform.TransformFinalBlock(array, 0, array.Length))
        Catch ex As Exception
            result = Nothing
        End Try
        Return result
    End Function

    Private Function HexStringToByteArray(key As String) As Byte()
        Dim result As Byte()
        Try
            Dim array As Byte() = New Byte(key.Length / 2 - 1 + 1 - 1) {}
            Dim num As Integer = key.Length - 1
            For i As Integer = 0 To num Step 2
                array(i / 2) = CByte(Convert.ToInt32(key.Substring(i, 2), 16))
            Next
            result = array
        Catch ex As Exception
            result = Nothing
        End Try
        Return result
    End Function
    Public Shared Function AESDecrypt2(ciphertext As String, key As String) As String
        Dim rijndaelManaged As RijndaelManaged = New RijndaelManaged()
        Dim sha256Cng As SHA256Cng = New SHA256Cng()
        Dim result As String
        Try
            Dim array As String() = ciphertext.Split(New Char() {"ÿ"c})
            Dim s As String = array(0)
            ciphertext = Decompress(array(1))
            rijndaelManaged.Key = sha256Cng.ComputeHash(Encoding.ASCII.GetBytes(key))
            rijndaelManaged.IV = Convert.FromBase64String(s)
            rijndaelManaged.Mode = CipherMode.CBC
            Dim cryptoTransform As ICryptoTransform = rijndaelManaged.CreateDecryptor()
            Dim array2 As Byte() = Convert.FromBase64String(ciphertext)
            Dim [string] As String = Encoding.UTF8.GetString(cryptoTransform.TransformFinalBlock(array2, 0, array2.Length))
            result = [string]
        Catch ex As Exception
            result = ""
        End Try
        Return result
    End Function
    Public Shared Function Decompress(compressedText As String) As String
        Dim array As Byte() = Convert.FromBase64String(compressedText)
        Dim [string] As String
        Using memoryStream As MemoryStream = New MemoryStream()
            Dim num As Integer = BitConverter.ToInt32(array, 0)
            memoryStream.Write(array, 4, array.Length - 4)
            Dim array2 As Byte() = New Byte(num - 1 + 1 - 1) {}
            memoryStream.Position = 0L
            Using gzipStream As GZipStream = New GZipStream(memoryStream, CompressionMode.Decompress)
                gzipStream.Read(array2, 0, array2.Length)
            End Using
            [string] = Encoding.UTF8.GetString(array2)
        End Using
        Return [string]
    End Function
    Private Sub InvoiceTaxUpdateError(clientCode As String, appConnectionString As String, tax_code As String, mst As String, mau_so As String, ky_hieu As String, so_hd As String, ngay_hd As String, userID As Integer, notError As Integer, Optional errorLog As String = "")
        Try
            Dim commandText As String = String.Format("exec Sthink$InputInvoiceTax$UpdateError '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', {7}, {8}, '{9}'", New Object() {clientCode, tax_code, mst, mau_so, ky_hieu, so_hd, ngay_hd, userID, notError, errorLog.Replace("'", "")})
            Me.Retrieve(commandText, True, appConnectionString)
        Catch ex As Exception
        End Try
    End Sub

    Private Function dtRetrieve(commandText As String, appConnectionString As String) As DataTable
        Try
            Return Me.Retrieve(commandText, True, appConnectionString).Tables(0)
        Catch ex As Exception
        End Try
        Return Nothing
    End Function

    Private Function Retrieve(commandText As String) As DataTable
        Try
            Return Me.Retrieve(commandText, False, "").Tables(0)
        Catch ex As Exception
        End Try
        Return Nothing
    End Function

    Private Function Retrieve(commandText As String, isAppConnect As Boolean, appConnectionString As String) As DataSet
        Dim result As DataSet
        Try
            Dim connectionString As String
            If isAppConnect Then
                connectionString = appConnectionString
            Else
                connectionString = Me.sysConnectionString
            End If
            result = Me.Retrieve(connectionString, commandText)
        Catch ex As Exception
            result = Nothing
        End Try
        Return result
    End Function

    Private Function Retrieve(connectionString As String, commandText As String) As DataSet
        Dim dataSet As DataSet = New DataSet()
        Try
            Dim factory As DbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient")
            Dim dbConnection As DbConnection = factory.CreateConnection()
            Dim dbCommand As DbCommand = factory.CreateCommand()
            Dim dbDataAdapter As DbDataAdapter = factory.CreateDataAdapter()
            dbConnection.ConnectionString = connectionString
            dbConnection.Open()
            Dim dbCommand2 As DbCommand = dbCommand
            dbCommand2.Connection = dbConnection
            dbCommand2.CommandText = commandText
            dbCommand2.CommandTimeout = Me.commandTimeout
            dbDataAdapter.SelectCommand = dbCommand
            dbDataAdapter.Fill(dataSet)
            dbConnection.Close()
            If Not Information.IsDBNull(dataSet) Then
                Return dataSet
            End If
        Catch ex As Exception
        End Try
        Return Nothing
    End Function

    Private Function RetrieveDataTable(connectionString As String, commandText As String) As DataTable
        Dim result As DataTable
        Try
            result = Me.Retrieve(connectionString, commandText).Tables(0)
        Catch ex As Exception
            result = Nothing
        End Try
        Return result
    End Function

    Private Sub Execute(connectionString As String, commandText As String)
        Try
            Dim sqlConnection As SqlConnection = New SqlConnection(connectionString)
            sqlConnection.Open()
            Dim sqlCommand As SqlCommand = New SqlCommand(commandText, sqlConnection)
            sqlCommand.CommandTimeout = 600
            Try
                sqlCommand.ExecuteNonQuery()
            Catch ex As Exception
                If sqlConnection.State = ConnectionState.Open Then
                    sqlConnection.Close()
                End If
            End Try
            If sqlConnection.State = ConnectionState.Open Then
                sqlConnection.Close()
            End If
        Catch ex2 As Exception
        End Try
    End Sub

    Private Sub WriteLogEntry(strError As String)
        Try
            Dim commandText As String = String.Format("insert into errorlog (ip, user_name, datetime0, message, error) select '{0}', '{1}', getdate(), '{2}', '{3}'", New Object() {"", "", "Tax Input Invoice", Strings.Left(Strings.Replace(strError, "'", "''", 1, -1, CompareMethod.Binary), 3990)})
            commandText += vbCr & " select '' as message"
            Dim connectionString As String = Me.sysConnectionString
            If String.IsNullOrEmpty(connectionString) Then
                Try
                    connectionString = WebConfigurationManager.ConnectionStrings("sysConnectionString").ConnectionString
                Catch ex As Exception
                End Try
            End If
            Me.Execute(connectionString, commandText)
        Catch ex2 As Exception
        End Try
    End Sub

    Private Function ClearString(string_6 As String) As String
        Dim result As String
        If Not String.IsNullOrEmpty(string_6) Then
            string_6 = string_6.Replace("'", "")
            string_6 = string_6.Replace(";", "")
            result = string_6
        Else
            result = ""
        End If
        Return result
    End Function

    Private Function BulkCopyData(string_6 As String, dataTable_0 As DataTable, string_7 As String, commandTextCreate As String, commandTextExec As String) As DataSet
        Dim result As DataSet
        Try
            Dim sqlConnection As SqlConnection = New SqlConnection(string_6)
            Dim sqlCommand As SqlCommand = sqlConnection.CreateCommand()
            sqlConnection.Open()
            sqlCommand.CommandText = commandTextCreate
            sqlCommand.ExecuteNonQuery()
            Using sqlBulkCopy As SqlBulkCopy = New SqlBulkCopy(sqlConnection)
                sqlBulkCopy.DestinationTableName = "#" + string_7
                sqlBulkCopy.WriteToServer(dataTable_0)
            End Using
            sqlCommand.CommandText = commandTextExec
            Dim dataSet As DataSet = New DataSet()
            Dim sqlDataReader As SqlDataReader = sqlCommand.ExecuteReader()
            Do
                Dim dataTable As DataTable = New DataTable()
                dataTable.Load(sqlDataReader)
                dataSet.Tables.Add(dataTable)
            Loop While Not sqlDataReader.IsClosed
            sqlConnection.Close()
            result = dataSet
        Catch ex As Exception
            result = Nothing
        End Try
        Return result
    End Function

    Private Function getConnection(sysConnectionString As String, clientCode As String) As String
        Dim result As String = ""
        Try
            Dim dataTable As DataTable = Me.RetrieveDataTable(sysConnectionString, String.Format("exec Sthink$InputInvoice$GetConnection '{0}'", clientCode))
            If dataTable IsNot Nothing AndAlso dataTable.Rows.Count > 0 Then
                result = dataTable.Rows(0)("connection").ToString().Trim()
            End If
        Catch ex As Exception
            result = ""
        End Try
        Return result
    End Function

    Private Sub GetAppSetting(clientCode As String, ByRef sysConnectionString As String, ByRef appConnectionString As String, appData As String)
        Dim path1 As String = "Web.config"
        Dim xmlDocument As XmlDocument = New XmlDocument()
        Dim xmlReaderSettings As XmlReaderSettings = New XmlReaderSettings()
        xmlReaderSettings.IgnoreComments = True
        Dim text As String
        If HttpContext.Current Is Nothing Then
            text = Path.Combine(Me.getCurrentDirectory(), path1)
            Me.appDatabaseName = appData
        Else
            text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), path1)
            Try
                Me.appDatabaseName = Conversions.ToString(Sthink.AjaxControlExtender.Session.GetSession(4))
            Catch ex As Exception
                Me.appDatabaseName = appData
            End Try
            If String.IsNullOrEmpty(Me.appDatabaseName) Then
                Me.appDatabaseName = appData
            End If
        End If
        Dim xmlReader As XmlReader = XmlReader.Create(text, xmlReaderSettings)
        xmlDocument.Load(xmlReader)
        Dim xmlNode_ As XmlNode = xmlDocument.SelectSingleNode("//appSettings")
        text = Conversions.ToString(Me.getItemXml(xmlNode_, "webConfigPath", Nothing))
        Me.networkKey = Conversions.ToString(Me.getItemXml(xmlNode_, "networkKey", Nothing))
        Me.sysDatabaseName = Conversions.ToString(Me.getItemXml(xmlNode_, "sysDatabaseName", Nothing))
        If Not String.IsNullOrEmpty(text) Then
            text = Path.Combine(text, "Web.config")
            Me.networkKey = Conversions.ToString(Me.getItemXml(xmlNode_, "networkKey", Nothing))
            xmlReader = XmlReader.Create(text, xmlReaderSettings)
            xmlDocument = New XmlDocument()
            xmlDocument.Load(xmlReader)
            xmlNode_ = xmlDocument.SelectSingleNode("//appSettings")
        End If
        Dim xmlNode As XmlNode = xmlDocument.SelectSingleNode("//connectionStrings")
        Me.commandTimeout = Conversions.ToInteger(Me.getItemXml(xmlNode_, "commandTimeout", Nothing))
        Me.invoiceTimeout = Conversions.ToInteger(Me.getItemXml(xmlNode_, "invoiceTimeout", Nothing))
        Me.invoiceService = Conversions.ToInteger(Me.getItemXml(xmlNode_, "invoiceService", Nothing))
        RuntimeHelpers.GetObjectValue(Me.getItemXml(xmlNode_, "sysDatabaseName", Nothing))
        If String.IsNullOrEmpty(Me.appDatabaseName) Then
            Try
                Me.appDatabaseName = Conversions.ToString(Me.getItemXml(xmlNode_, "appDatabaseName", Nothing))
            Catch ex2 As Exception
            End Try
        End If
        Dim num As Integer = xmlNode.ChildNodes.Count - 1
        For i As Integer = 0 To num
            Dim value As String = xmlNode.ChildNodes(i).Attributes("name").Value
            If Operators.CompareString(value, "sysConnectionString", False) = 0 Then
                sysConnectionString = xmlNode.ChildNodes(i).Attributes("connectionString").Value
            End If
        Next
        If clientCode.Length > 6 Then
            clientCode = AESDecrypt2(clientCode, Me.networkKey)
            Me.appDatabaseName = AESDecrypt2(Me.appDatabaseName, Me.networkKey)
        End If
        Dim text2 As String = Me.getConnection(sysConnectionString, clientCode)
        If Not String.IsNullOrEmpty(text2) Then
            Dim num2 As Integer = xmlNode.ChildNodes.Count - 1
            For j As Integer = 0 To num2
                If Operators.CompareString(xmlNode.ChildNodes(j).Attributes("name").Value, text2, False) = 0 Then
                    appConnectionString = xmlNode.ChildNodes(j).Attributes("connectionString").Value
                    appConnectionString = appConnectionString.Replace("%Database", Me.appDatabaseName)
                End If
            Next
        End If
        xmlReader.Close()
    End Sub

    Private Function getItemXml(xmlNode As XmlNode, key As String, Optional objectDefault As Object = Nothing) As Object
        Try
            For Each obj As Object In xmlNode
                Dim _xmlNode As XmlNode = CType(obj, XmlNode)
                If Operators.CompareString(_xmlNode.Attributes("key").Value, key, False) = 0 Then
                    Return _xmlNode.Attributes("value").Value
                End If
            Next
        Finally
            Dim enumerator As IEnumerator
            If TypeOf enumerator Is IDisposable Then
                TryCast(enumerator, IDisposable).Dispose()
            End If
        End Try
        Dim result As Object
        If objectDefault Is Nothing Then
            result = ""
        Else
            result = objectDefault
        End If
        Return result
    End Function

    Private Function getCurrentDirectory() As String
        Dim text As String = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName
        If Operators.CompareString(Strings.Right(text, 1), "\", False) = 0 Then
            text = Strings.Left(text, text.Length - 1)
        End If
        Return Directory.GetParent(text).FullName + "\"
    End Function

    Private sysDatabaseName As String

    Private appDatabaseName As String

    Private sysConnectionString As String

    Private commandTimeout As Integer

    Private invoiceTimeout As Integer

    Private networkKey As String

    Private tblTmp As String

    Private invoiceService As Integer
End Class
