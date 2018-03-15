using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace Hibernate_PersistenceApi.Facts
{
    public class TestBase
    {
        readonly ISessionFactory sessionFactory;

        public TestBase()
        {
            MsSqlConfiguration persistenceConfigurer = MsSqlConfiguration
                .MsSql2008
                .ConnectionString("Data Source=localhost;Initial Catalog=TestDb;Integrated Security=True;Enlist=false")
                .Raw("connection.release_mode", "on_close");
            sessionFactory = Fluently
                .Configure()
                .Database(persistenceConfigurer)
                .Mappings(m => m.FluentMappings.AddFromAssembly(typeof(User).Assembly))
                .BuildSessionFactory();
        }

        protected ISession OpenSession()
        {
            ISession session = sessionFactory.OpenSession();
            session.FlushMode = FlushMode.Manual;
            return session;
        }
    }
}