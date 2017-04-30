using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace Iwenli.AspNetServer
{

    [SuppressUnmanagedCodeSecurity]
    internal sealed class NtlmAuth : IDisposable
    {
        private string m_blob;
        private bool m_completed;
        private SecHandle m_credentialsHandle;
        private bool m_credentialsHandleAcquired;
        private SecBuffer m_inputBuffer;
        private SecBufferDesc m_inputBufferDesc;
        private SecBuffer m_outputBuffer;
        private SecBufferDesc m_outputBufferDesc;
        private SecHandle m_securityContext;
        private bool m_securityContextAcquired;
        private uint m_securityContextAttributes;
        private SecurityIdentifier m_sid;
        private long m_timestamp;

        private const int ISC_REQ_ALLOCATE_MEMORY = 0x100;
        private const int ISC_REQ_CONFIDENTIALITY = 0x10;
        private const int ISC_REQ_DELEGATE = 1;
        private const int ISC_REQ_MUTUAL_AUTH = 2;
        private const int ISC_REQ_PROMPT_FOR_CREDS = 0x40;
        private const int ISC_REQ_REPLAY_DETECT = 4;
        private const int ISC_REQ_SEQUENCE_DETECT = 8;
        private const int ISC_REQ_STANDARD_FLAGS = 20;
        private const int ISC_REQ_USE_SESSION_KEY = 0x20;
        private const int ISC_REQ_USE_SUPPLIED_CREDS = 0x80;
        private const int SEC_E_OK = 0;
        private const int SEC_I_COMPLETE_AND_CONTINUE = 0x90314;
        private const int SEC_I_COMPLETE_NEEDED = 0x90313;
        private const int SEC_I_CONTINUE_NEEDED = 0x90312;
        private const int SECBUFFER_DATA = 1;
        private const int SECBUFFER_EMPTY = 0;
        private const int SECBUFFER_TOKEN = 2;
        private const int SECBUFFER_VERSION = 0;
        private const int SECPKG_CRED_INBOUND = 1;
        private const int SECURITY_NETWORK_DREP = 0;

        public NtlmAuth()
        {
            if (AcquireCredentialsHandle(null, "NTLM", 1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref this.m_credentialsHandle, ref this.m_timestamp) != 0)
            {
                throw new InvalidOperationException();
            }
            this.m_credentialsHandleAcquired = true;
        }

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        private static extern int AcceptSecurityContext(ref SecHandle phCredential, IntPtr phContext, ref SecBufferDesc pInput, uint fContextReq, uint TargetDataRep, ref SecHandle phNewContext, ref SecBufferDesc pOutput, ref uint pfContextAttr, ref long ptsTimeStamp);
        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        private static extern int AcquireCredentialsHandle(string pszPrincipal, string pszPackage, uint fCredentialUse, IntPtr pvLogonID, IntPtr pAuthData, IntPtr pGetKeyFn, IntPtr pvGetKeyArgument, ref SecHandle phCredential, ref long ptsExpiry);
        public unsafe bool Authenticate(string blobString)
        {
            this.m_blob = null;
            byte[] buffer = Convert.FromBase64String(blobString);
            byte[] inArray = new byte[0x4000];
            fixed (SecHandle* voidRef = &this.m_securityContext)
            {
                fixed (SecBuffer* voidRef2 = &this.m_inputBuffer)
                {
                    fixed (SecBuffer* voidRef3 = &this.m_outputBuffer)
                    {
                        fixed (void* voidRef4 = buffer)
                        {
                            fixed (void* voidRef5 = inArray)
                            {
                                IntPtr zero = IntPtr.Zero;
                                if (this.m_securityContextAcquired)
                                {
                                    zero = (IntPtr)voidRef;
                                }
                                this.m_inputBufferDesc.ulVersion = 0;
                                this.m_inputBufferDesc.cBuffers = 1;
                                this.m_inputBufferDesc.pBuffers = (IntPtr)voidRef2;
                                this.m_inputBuffer.cbBuffer = (uint)buffer.Length;
                                this.m_inputBuffer.BufferType = 2;
                                this.m_inputBuffer.pvBuffer = (IntPtr)voidRef4;
                                this.m_outputBufferDesc.ulVersion = 0;
                                this.m_outputBufferDesc.cBuffers = 1;
                                this.m_outputBufferDesc.pBuffers = (IntPtr)voidRef3;
                                this.m_outputBuffer.cbBuffer = (uint)inArray.Length;
                                this.m_outputBuffer.BufferType = 2;
                                this.m_outputBuffer.pvBuffer = (IntPtr)voidRef5;
                                int num = AcceptSecurityContext(ref this.m_credentialsHandle, zero, ref this.m_inputBufferDesc, 20, 0, ref this.m_securityContext, ref this.m_outputBufferDesc, ref this.m_securityContextAttributes, ref this.m_timestamp);
                                if (num == 0x90312)
                                {
                                    this.m_securityContextAcquired = true;
                                    this.m_blob = Convert.ToBase64String(inArray, 0, (int)this.m_outputBuffer.cbBuffer);
                                }
                                else
                                {
                                    if (num != 0)
                                    {
                                        return false;
                                    }
                                    IntPtr phToken = IntPtr.Zero;
                                    if (QuerySecurityContextToken(ref this.m_securityContext, ref phToken) != 0)
                                    {
                                        return false;
                                    }
                                    try
                                    {
                                        using (WindowsIdentity identity = new WindowsIdentity(phToken))
                                        {
                                            this.m_sid = identity.User;
                                        }
                                    }
                                    finally
                                    {
                                        CloseHandle(phToken);
                                    }
                                    this.m_completed = true;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }


        [DllImport("KERNEL32.DLL", CharSet = CharSet.Unicode)]
        private static extern int CloseHandle(IntPtr phToken);
        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        private static extern int DeleteSecurityContext(ref SecHandle phContext);
        ~NtlmAuth()
        {
            this.FreeUnmanagedResources();
        }

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        private static extern int FreeCredentialsHandle(ref SecHandle phCredential);
        private void FreeUnmanagedResources()
        {
            if (this.m_securityContextAcquired)
            {
                DeleteSecurityContext(ref this.m_securityContext);
            }
            if (this.m_credentialsHandleAcquired)
            {
                FreeCredentialsHandle(ref this.m_credentialsHandle);
            }
        }

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        private static extern int QuerySecurityContextToken(ref SecHandle phContext, ref IntPtr phToken);
        void IDisposable.Dispose()
        {
            this.FreeUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public string Blob
        {
            get
            {
                return this.m_blob;
            }
        }

        public bool Completed
        {
            get
            {
                return this.m_completed;
            }
        }

        public SecurityIdentifier SID
        {
            get
            {
                return this.m_sid;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecBuffer
        {
            public uint cbBuffer;
            public uint BufferType;
            public IntPtr pvBuffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecBufferDesc
        {
            public uint ulVersion;
            public uint cBuffers;
            public IntPtr pBuffers;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecHandle
        {
            public IntPtr dwLower;
            public IntPtr dwUpper;
        }

    }
}
