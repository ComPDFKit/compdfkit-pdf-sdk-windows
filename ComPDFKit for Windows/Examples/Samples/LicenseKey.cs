using ComPDFKit.NativeMethod;
using static ComPDFKit.NativeMethod.CPDFSDKVerifier;

public static class SDKLicenseHelper
{ 
    public static bool LicenseVerify()
    {
        if (!LoadNativeLibrary())
            return false;

        string license = "24ywO5QOL/6xA78UdefdSqyCN2DAK0GO3ctq7PFduZ7jNyK2r8ru+OkzxO/pRmyc/SdRk4qZpjcTuzVWqlvm9mYZYcz8fc0CCKLl7jziHKTdiudWddyaWwABRMRrw2IxMCT+UrngegMZ7KVzurK76fsaX0ULRsCR41Bd0UoPaybvGBeuVs3I6XTP5E5Hu0fpCo+oUwVV6xNJyIkK/B7oazkC0pjJFMnoNZ8fKI3M+P0jza6dpM6wQkM+b9GqGKu0XjKBE+d5OJgXD4oRIXKAYT86jmtum1ZcLYItWK9ChIO9TQJYuvtcIDCSnrNuA0YQegnfoY/q5b+rn7PQDV5di0tTVN6c+2dRhnnKQ9BD2+ZXPq1h0mtlp1NQ+RMIko/jptqMsDODbroq9eCcyCqv15famjsc5QhApxJ66Uir6JIEWg+1gHSh2bjFiiXJAZ6NYxZRbQCMGNWAvkvPL3VOmCcPKDpJojB4dAuUzkjcfNP3FtGWASLlf1sxBLPPUH3/SUjuKo61mV+inIkdPNQcpTuQO57aUzB8KSNTD9t5EApfDx1B3KqboczEI8JHpWmS+IJqLCfsdlZLlGqIVobinsWoWlrK+RCjMLVb1nG6cwjM5Mqll1bnX2z0pJMoAOF3kWgbT7lJD0GQGywLcuHOOrC478hvP2cDIaltVnym8VrDlbODXEho1HLWbJp0z6qlTJ6nD4pqlkYW0VU+Uu1LE2VCymCuLT01rJlnne9TsMGmX5tTLvKy9ITFm7uwkVffejxBTHIGmDrTVqlc7ABbYzXhlraeCUwg0QL0p8RvjNrLZiQUN8OqBJvMBbPRBvWxV0Fn6PIJWs5RPb0ewCk388ll85WKAtCYrFc+McXDZMpeW3W15O6SZ7ytdH0UNznOvCBEtlaG/qQYCIY/Mi23F98HL3lGbW1p+twFA05ez45CEe7JdAVzpWUI2KWlr3c5V7JB9xcxW0WhhJkt8nd55b6LfhfPoH5/z+YfXo5CvTFi9iZhMizKIZvrFgGf7a/v+u16tzCiO1ZkOjwxr2Hr5w==";
        LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(license, false);
        return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
    }
}
