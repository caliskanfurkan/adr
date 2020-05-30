using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Diagnostics.Symbols;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;


namespace adr
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!(TraceEventSession.IsElevated() ?? false))
            {
                Console.WriteLine("Admin değilsin");
                return;
            }

            using (var kernelSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName))
            {
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) { kernelSession.Dispose(); };

                kernelSession.EnableKernelProvider(
                    KernelTraceEventParser.Keywords.Process |
                    KernelTraceEventParser.Keywords.NetworkTCPIP |
                    KernelTraceEventParser.Keywords.ImageLoad |
                    KernelTraceEventParser.Keywords.Registry
                );

                kernelSession.Source.Kernel.ProcessStart += _processStart;
                kernelSession.Source.Kernel.TcpIpConnect += _tcpMetod;
                kernelSession.Source.Kernel.ImageLoad += _imageLoad;
                kernelSession.Source.Kernel.RegistryCreate += _regCreate;

                kernelSession.Source.Process();
            }
        }

        private static void _processStart(ProcessTraceData pData)
        {
            Console.WriteLine("Process başladı:\tProcessID: {0}\tProcess ParentID: {1}\tProcessName: {2}\tKomut Satırı: {3}", pData.ProcessID, pData.ParentID, 
             pData.ProcessName, pData.CommandLine);
        }

        private static void _regCreate(RegistryTraceData rData)
        {
            Console.WriteLine("Registry operasyonu:\tProcessName: {0}\tKey adı: {1}\tValueName: {2}", rData.ProcessName, rData.KeyName, rData.ValueName);
        }


        private static void _tcpMetod(TcpIpConnectTraceData ipTraceData) {
           Console.WriteLine("Network Bağlantısı:\tProcess ID: {0}\tProcess Adı: {1}\tBağlanılan IP:{2}\tBağlanılan Port:{3}",
            ipTraceData.ProcessID, ipTraceData.ProviderName, ipTraceData.daddr, ipTraceData.dport);
        }

        private static void _imageLoad(ImageLoadTraceData pModuleData)
        {
            Console.WriteLine("Modül Yükleme Faaliyeti:\tProcessName: {0}\tFile Name {1}", pModuleData.ProcessName, pModuleData.FileName);
        }
    }
}
