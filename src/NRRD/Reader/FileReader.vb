Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Net.Http

Public Class FileReader : Implements IDisposable

    ReadOnly file As Stream
    ReadOnly header As Header
    ReadOnly comments As New List(Of String)

    Public ReadOnly Property NrddHeader As Metadata
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return header.toMetadata
        End Get
    End Property

    Dim disposedValue As Boolean
    Dim scan0 As Long

    Sub New(file As Stream)
        Me.file = file
        Me.header = loadNrrdHeader()
    End Sub

    Public Function LoadRaster()
        Dim bytes As New BinaryDataReader(loadNrrdRawBuffer)
        Dim data As Array

        ' the source stream is loaded from
        ' the nrdd file substream
        ' the actually scan0 is ZERO
        bytes.Seek(0, SeekOrigin.Begin)
        data = BytesBuffer.parseNRRDRawData(bytes, header.toMetadata)


    End Function

    Private Function loadNrrdRawBuffer() As MemoryStream
        Dim size As Integer = file.Length - scan0
        Dim bytes As Byte() = New Byte(size - 1) {}
        Dim metadata As Metadata = header.toMetadata

        file.Seek(scan0, SeekOrigin.Begin)
        file.Read(bytes, scan0, bytes.Length)

        Select Case metadata.encoding
            Case Encoding.raw : Return New MemoryStream(bytes)
            Case Encoding.gzip, Encoding.gz
                Return GZipStreamHandler.UnGzipStream(bytes)
            Case Else
                Throw New NotImplementedException(metadata.encoding)
        End Select
    End Function

    Private Function loadNrrdHeader() As Header
        Dim read As New StreamReader(file)
        Dim magic As String = read.ReadLine
        Dim line As Value(Of String) = ""
        Dim metadata As NamedValue(Of String)
        Dim header As New Header With {
            .magicNumber = [Enum].Parse(GetType(MagicNumber), magic),
            .metadata = New Dictionary(Of String, String)
        }

        Do While Not (line = read.ReadLine).StringEmpty
            If line.First = "#"c Then
                comments.Add(line)
            Else
                metadata = line.GetTagValue(":", trim:=True)
                header.add(metadata.Name, metadata.Value)
            End If
        Loop

        scan0 = file.Position

        Return header
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
                Call file.Close()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class