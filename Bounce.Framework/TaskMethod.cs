using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bounce.Framework {
    public class TaskMethod : ITask {
        private readonly MethodInfo Method;
        private readonly IDependencyResolver Resolver;

        public TaskMethod(MethodInfo method, IDependencyResolver resolver) {
            Method = method;
            Resolver = resolver;
        }

        public string Name {
            get { return Method.Name; }
        }

        public string FullName {
            get { return Method.DeclaringType.FullName + "." + Method.Name; }
        }

        public void Invoke(Arguments arguments) {
            try {
                var taskObject = Resolver.Resolve(Method.DeclaringType);
                var methodArguments = MethodArgumentsFromCommandLineParameters(arguments);
                Method.Invoke(taskObject, methodArguments);
            } catch (TargetInvocationException e) {
                throw new TaskException(this, e.InnerException);
            }
        }

        private object[] MethodArgumentsFromCommandLineParameters(Arguments arguments)
        {
            return Parameters.Select(p => (object)ParseParameter(arguments, p)).ToArray();
        }

        private object ParseParameter(Arguments arguments, ITaskParameter p)
        {
            try {
                return arguments.Parameter(p);
            } catch(RequiredParameterNotGivenException e) {
                throw new TaskRequiredParameterException(p, this);
            } catch (TypeParserNotFoundException e) {
                throw new TaskParameterException(p, this);
            }
        }

        public IEnumerable<string> AllNames {
            get {
                var fullName = FullName;

                yield return fullName;
                int index = fullName.IndexOf(".");
                while (index > 0)
                {
                    fullName = fullName.Substring(index + 1);
                    yield return fullName;
                    index = fullName.IndexOf(".");
                }
            }
        }

        public IEnumerable<ITaskParameter> Parameters {
            get { return Method.GetParameters().Select(p => (ITaskParameter) new TaskMethodParameter(p)); }
        }
    }
}