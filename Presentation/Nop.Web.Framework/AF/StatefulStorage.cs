using System;
using System.Collections;
using System.Web;

namespace Nop.Web.Framework
{
    //AF version overrides all inerface, implementation of 
    public interface IStatefulStorage
    {
        TValue Get<TValue>(string name);
        TValue GetOrAdd<TValue>(string name, Func<TValue> valueFactory);
        TValue Add<TValue>(string name, Func<TValue> valueFactory);
        void Remove<TValue>(string name);
    }

    public static class StatefulStorage
    {
        public static IStatefulStorage PerApplication
        {
            get { return new StatefulStoragePerApplication(); }
        }

        public static IStatefulStorage PerRequest
        {
            get { return new StatefulStoragePerRequest(); }
        }

        public static IStatefulStorage PerSession
        {
            get { return new StatefulStoragePerSession(); }
        }
    }

    public class StatefulStoragePerApplication : DictionaryStatefulStorage
    {
        // Ambient environment constructor
        public StatefulStoragePerApplication()
            : base((key) => HttpContext.Current.Application[key],
                   (key, value) => HttpContext.Current.Application[key] = value) { }

        // IoC-friendly constructor
        public StatefulStoragePerApplication(HttpApplicationStateBase app)
            : base((key) => app[key],
                   (key, value) => app[key] = value) { }
    }

    public class StatefulStoragePerRequest : DictionaryStatefulStorage
    {
        // Ambient environment constructor
        public StatefulStoragePerRequest()
            : base(() => HttpContext.Current.Items) { }

        // IoC-friendly constructor
        public StatefulStoragePerRequest(HttpContextBase context)
            : base(() => context.Items) { }
    }

    public class StatefulStoragePerSession : DictionaryStatefulStorage
    {
        // Ambient environment constructor
        public StatefulStoragePerSession()
            : base((key) => HttpContext.Current.Session[key],
                   (key, value) => HttpContext.Current.Session[key] = value,
            (key) => HttpContext.Current.Session.Remove(key)
            ) { }

        // IoC-friendly constructor
        public StatefulStoragePerSession(HttpSessionStateBase session)
            : base((key) => session[key],
                   (key, value) => session[key] = value,
            (key) => session.Remove(key)
            ) { }
    }

    public abstract class DictionaryStatefulStorage : IStatefulStorage
    {
        Func<string, object> getter;
        Action<string, object> setter;
        Action<string> remover;

        protected DictionaryStatefulStorage(Func<IDictionary> dictionaryAccessor)
        {
            getter = (key) => dictionaryAccessor()[key];
            setter = (key, value) => dictionaryAccessor()[key] = value;
            remover = (key) => dictionaryAccessor().Remove(key);
        }
        protected DictionaryStatefulStorage(Func<string, object> getter, Action<string, object> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        protected DictionaryStatefulStorage(Func<string, object> getter, Action<string, object> setter, Action<string> remover)
            :this(getter,setter)
        {
            this.remover = remover;
        }

        protected static string FullNameOf(Type type, string name)
        {
            string fullName = type.FullName;
            if (!String.IsNullOrWhiteSpace(name))
                fullName += "::" + name;

            return fullName;
        }

        public TValue Get<TValue>(string name)
        {
            return (TValue)getter(FullNameOf(typeof(TValue), name));
        }

        public TValue GetOrAdd<TValue>(string name, Func<TValue> valueFactory)
        {
            string fullName = FullNameOf(typeof(TValue), name);
            TValue result = (TValue)getter(fullName);

            if (Object.Equals(result, default(TValue)))
            {
                result = valueFactory();
                setter(fullName, result);
            }

            return result;
        }

        public TValue Add<TValue>(string name, Func<TValue> valueFactory)
        {
            string fullName = FullNameOf(typeof(TValue), name);
            TValue result = valueFactory();
                setter(fullName, result);

            return result;
        }

        public void Remove<TValue>(string name)
        {
            string fullName = FullNameOf(typeof(TValue), name);
            remover(fullName);
        }
    }

    public static class StatefulStorageExtensions
    {
        public static TValue Get<TValue>(this IStatefulStorage storage)
        {
            return storage.Get<TValue>(name: null);
        }

        public static TValue GetOrAdd<TValue>(this IStatefulStorage storage, Func<TValue> valueFactory)
        {
            return storage.GetOrAdd<TValue>(name: null, valueFactory: valueFactory);
        }

        public static TValue Add<TValue>(this IStatefulStorage storage, Func<TValue> valueFactory)
        {
            return storage.Add<TValue>(name: null, valueFactory: valueFactory);
        }
    }
}
