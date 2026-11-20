using System;

namespace Dive.Core.Common
{
    /// <summary>
    /// Thrown when a deployment step fails. Aborts the deployment; <see cref="Step"/>
    /// tells the user interface which step was affected.
    /// </summary>
    public class DeploymentException : Exception
    {
        public Progress Step { get; }

        public DeploymentException(Progress step, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Step = step;
        }
    }
}
