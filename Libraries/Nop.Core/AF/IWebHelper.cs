using System;
namespace Nop.Core
{
    partial interface IWebHelper
    {
        string GetAbsolutePath(string relativePath);
    }
}
