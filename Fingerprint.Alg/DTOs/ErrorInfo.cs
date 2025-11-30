namespace Fingerprint.Alg.DTOs;

public class ErrorInfo
{
    public string Message { get; set; }

    public StackFrameInfo[] Trace { get; set; }
}