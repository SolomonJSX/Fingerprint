namespace Fingerprint.Core.DTOs;

public class ErrorInfo
{
    public string Message { get; set; }

    public StackFrameInfo[] Trace { get; set; }
}