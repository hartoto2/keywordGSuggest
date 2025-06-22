
Imports System.Net
Imports System.IO

Imports System.Xml


Module Module1

    Private oListAlphaFile As New List(Of String)
    Private iAlphaFileIndex As Integer
    Private iFileGet As Integer
    Private iSuggestInFile10 As Integer


    Private oListKwByAlpha As New List(Of String)

    Private sArrAlphabet() As String = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", _
                                        "k", "l", "m", "n", "o", "p", _
                                        "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"}

    Private k As Integer


    Private timerSuggest As New System.Timers.Timer
    Private timerFileGet As New System.Timers.Timer

    Private timerSuggestInFile10 As New System.Timers.Timer
    Private timerSuggestByAlphabet26 As New System.Timers.Timer

    Private iKwIndex As Integer

    Private iSuggest As Integer
    Private iSuggestByAlpha As Integer
    Private iSuggestByAlpha26 As Integer

    Private sCountry As String = ""


    Sub Main()

        Console.SetWindowSize(Console.WindowWidth / 2, Console.WindowHeight / 2)


        AddHandler timerSuggest.Elapsed, AddressOf tickSuggest
        timerSuggest.Interval = getDelay()

        AddHandler timerFileGet.Elapsed, AddressOf tickFileGet
        timerFileGet.Interval = getDelay()
       
        AddHandler timerSuggestInFile10.Elapsed, AddressOf tickSuggestInFile10
        timerSuggestInFile10.Interval = getDelay()

        AddHandler timerSuggestByAlphabet26.Elapsed, AddressOf tickSuggestByAlphabet26
        timerSuggestByAlphabet26.Interval = getDelay()


        Console.WriteLine("Google Suggest v1.0 (c)XBot 2021")
        Console.WriteLine("--------------------------------")
        Console.WriteLine("")

        Threading.Thread.Sleep(1000)

        sCountry = IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory & "country.txt")
        Dim sKeyword As String = IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory & "keyword.txt")
        ' Console.WriteLine("sCountry: " + sCountry)
        Console.WriteLine("sKeyword: " + sKeyword)
        Console.WriteLine("")
        Console.WriteLine("wait for google suggest...")
        Console.WriteLine("")

        ' Dim sKw As String = IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectofry & "keyword.txt")
        'suggestGet(sKw)


        IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory & "kw_0results.txt", "")
        '-- clear kw_alphabet file

        For i = 0 To sArrAlphabet.Count - 1
            Dim sAlpha As String = getAlphabet(i).ToString
            If Trim(sAlpha) <> "" Then
                IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory & "kw_" & sAlpha & ".txt", "")
                oListAlphaFile.Add("kw_" & sAlpha & ".txt")
            End If
        Next

        'For Each item In oListAlphaFile
        '    Console.WriteLine(item)
        'Next
        timerSuggest.Start()


        Console.ReadKey()

    End Sub


    Private Function getDelay()
        Dim iDelay As Integer = CInt(IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory & "delay.txt"))
        Return iDelay * 1000
    End Function

    Public Sub suggestGet(ByVal sKw As String, ByVal idxAlphabet As Integer, Optional byAlpha As Integer = 0)

        '--- GET https://www.googleapis.com/blogger/v3/blogs/blogId/posts
        '--- GET https://www.googleapis.com/blogger/v3/blogs/1743935507371290094/posts?key={YOUR_API_KEY}

        'sAccess_Token_Blogspot = sAccess_Token_Blogspot
        ServicePointManager.Expect100Continue = True
        '   ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls1

        Try

            Dim sUrl As String = "http://suggestqueries.google.com/complete/search?output=xml&hl=lang_en"

            If sCountry <> "xx" Then
                sUrl = sUrl & "&gl = " & sCountry
            End If
            sUrl = sUrl & "&q=" & sKw

            ' Console.WriteLine("blogpotList.UrlReq: " & sUrl)

            Dim request As WebRequest = Nothing
            Dim webAddress As Uri
            webAddress = New Uri(sUrl)

            'txtUrlReq.Text = sUrl

            ' Create the web request  
            request = DirectCast(WebRequest.Create(webAddress), HttpWebRequest)
            Dim myheaders As WebHeaderCollection

            ' set type to GET
            request.Method = "GET"
            request.ContentType = "application/json"
            myheaders = request.Headers
            myheaders.Add("charset", "utf-8")

            '----- get response
            Dim response As WebResponse = request.GetResponse()
            'MsgBox(CType(response, HttpWebResponse).StatusDescription)

            Dim dataStream As Stream = response.GetResponseStream()
            Dim reader As New StreamReader(dataStream)
            Dim responseFromServer As String = reader.ReadToEnd()

            '  Console.WriteLine("response:")
            ' Console.WriteLine(responseFromServer)
            'Console.WriteLine("")
            parsingXML(responseFromServer, idxAlphabet, byAlpha)

        Catch ex As Exception

            Console.WriteLine(ex.ToString)
        End Try

    End Sub


    Private Sub parsingXML(ByVal sXml As String, ByVal idxAlphabet As Integer, Optional byAlpha As Integer = 0)

        Dim oXML As New XmlDocument
        oXML.LoadXml(sXml)

        Dim oNode As XmlNode = oXML.ChildNodes(1)

        ' Console.WriteLine(oNode.ChildNodes.Count)
        Dim sItem As String
        k = k + 1
        For i = 0 To oNode.ChildNodes.Count - 1
            sItem = oNode.ChildNodes(i).ChildNodes(0).Attributes("data").InnerText



            If byAlpha = 0 Then

                Dim sAlpha As String = getAlphabet(idxAlphabet).ToString

                If Trim(sAlpha) <> "" Then
                    Dim sFileName As String = AppDomain.CurrentDomain.BaseDirectory & "kw_" & sAlpha & ".txt"
                    If i * k = 0 Then
                        Console.WriteLine("")
                        Console.WriteLine(" -> " & " kw_" & sAlpha & ".txt")
                    End If

                    IO.File.AppendAllText(sFileName, sItem & vbCrLf)

                    If isRecExist(sItem) = False Then
                        IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory & "kw_0results.txt", sItem & vbCrLf)
                    End If

                End If
            End If

            If byAlpha = 1 Then
                If isRecExist(sItem) = False Then
                    IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory & "kw_0results.txt", sItem & vbCrLf)
                End If
            End If

            Dim arrResultKw() As String = IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory & "kw_0results.txt").Split(vbLf)
            Dim iKwCount As Integer = arrResultKw.Length - 1
            Console.WriteLine(sItem & " (" & iKwCount.ToString & ")")

        Next

    End Sub

   
    Private Function getAlphabet(ByVal idx As Integer) As String


        Dim sa As String = ""
        If idx <= 26 Then
            sa = sArrAlphabet(idx)
        Else
            ' Console.WriteLine("finish!")
        End If

        Return sa


    End Function

    Private Sub tickSuggest()

        iSuggest = iSuggest + 1

        If iSuggest - 1 <= 26 Then
            Dim sKw As String =
                IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory & "keyword.txt")

            sKw = sKw & Space(1) & getAlphabet(iSuggest - 1)
            suggestGet(sKw, iSuggest - 1, 0)
        Else
            timerSuggest.Stop()
            timerFileGet.Start()
        End If

    End Sub


    Private Sub scrapKwByAlphabet(ByVal idxFile As Integer)

        'For Each item In oListAlphaFile
        '    Console.WriteLine(item)
        'Next

        Dim sFile As String = oListAlphaFile(idxFile)
        Console.WriteLine("")
        Console.WriteLine("sFile: " & sFile)
        Dim sArrKw() As String = IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory & sFile).Split(vbLf)

        oListKwByAlpha.Clear()
        For Each sKw As String In sArrKw
            sKw = sKw.Replace(vbLf, "").Replace(vbCr, "").Replace(vbCrLf, "")
            ' Console.WriteLine("sKw: " & sKw)
            If Trim(sKw) <> "" Then
                oListKwByAlpha.Add(sKw)
            End If
        Next

    End Sub


    Private Sub tickFileGet()

        iFileGet = iFileGet + 1

        Console.WriteLine("iFileGet: " & iFileGet)
        If iFileGet - 1 <= 26 Then
            scrapKwByAlphabet(iFileGet - 1)

            Console.WriteLine("")
            timerFileGet.Stop()
            timerSuggestInFile10.Start()
        Else
            iFileGet = 0
        End If

    End Sub

    Private Sub tickSuggestInFile10()

        iSuggestInFile10 = iSuggestInFile10 + 1
        'Console.WriteLine("oListKwByAlpha.Count: " & oListKwByAlpha.Count)

        If iSuggestInFile10 <= 10 Then
            Dim skw As String = oListKwByAlpha(iSuggestInFile10 - 1)
            Console.WriteLine("skw: " & skw)
            Console.WriteLine("")
            timerSuggestByAlphabet26.Start()
            timerSuggestInFile10.Stop()
        Else
            iSuggestInFile10 = 0
            timerFileGet.Start()
            timerSuggestInFile10.Stop()
        End If

    End Sub


    Private Sub tickSuggestByAlphabet26()

        iSuggestByAlpha26 = iSuggestByAlpha26 + 1

        If iSuggestByAlpha26 <= 26 Then
            Dim skw As String = oListKwByAlpha(iSuggestInFile10 - 1)
            skw = skw & Space(1) & getAlphabet(iSuggestByAlpha26 - 1)
            Console.WriteLine("skw: " & skw)
            Console.WriteLine("")
            suggestGet(skw, iSuggestByAlpha26 - 1, 1)
        Else
            iSuggestByAlpha26 = 0
            timerSuggestByAlphabet26.Stop()
            timerSuggestInFile10.Start()
        End If

    End Sub

    Private Function isRecExist(ByVal sKw As String) As Boolean

        Dim bExist As Boolean = False
        Dim sArrKw() As String = IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory & "kw_0results.txt").Split(vbLf)
        For Each item In sArrKw
            item = item.Replace(vbLf, "").Replace(vbCr, "").Replace(vbCrLf, "")
            If sKw = item Then
                Console.WriteLine(sKw & " <- already exist!")
                bExist = True
                Exit For
            End If
        Next

        Return bExist

    End Function

End Module
