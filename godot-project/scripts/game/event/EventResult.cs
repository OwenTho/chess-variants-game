using System;

[Flags]
public enum EventResult
{
    None = 0,
    Continue = 1,
    Wait = 2,
    Cancel = 4
}