using System.CommandLine;
using Microsoft.Extensions.Hosting;

namespace Fhi.Security.Cryptography.CLI.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        Command Build(IHost host);
    }
}
