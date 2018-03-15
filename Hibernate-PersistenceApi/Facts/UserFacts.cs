using NHibernate;
using Xunit;

namespace Hibernate_PersistenceApi.Facts
{
    public class UserFacts : TestBase
    {
        public UserFacts()
        {
            using (ISession session = OpenSession())
            {
                session.CreateSQLQuery("TRUNCATE TABLE users").UniqueResult();
            }
        }

        /*
         * 1. save transient user
         * 2. save persistent user
         * 3. save detached user
         */
        [Fact]
        public void should_save_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                var userId = (long) session.Save(user);
                Assert.NotEqual(0, userId);
                Assert.NotEqual(0, user.Id);
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                user.Name = "Zhen";
                session.Save(user);
            }

            using (ISession session = OpenSession())
            {
                user.Name = "Guang";
                session.Save(user);
            }
        }

        /*
         * 1. persistent transient user
         * 2. persistent persistent user
         * 3. persistent detached user
         */
        [Fact]
        public void should_persistent_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                session.Persist(user);
                Assert.NotEqual(0, user.Id);
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                user.Name = "Zhen";
                session.Persist(user);
            }

            using (ISession session = OpenSession())
            {
                user.Name = "Guang";
                session.Persist(user);
            }
        }

        [Fact]
        public void should_update_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                session.Save(user);

                user.Name = "Zhen";
                session.Flush();
                Assert.Equal("Zhen", user.Name);
            }

            var detachedUser = new User { Id = user.Id, Name = "Zhu" };
            using (ISession session = OpenSession())
            {
                session.Update(detachedUser);
                //Assert.Equal("", detachedUser.Name);

                detachedUser.Name = "Guang";
                session.Flush();
            }
        }
    }
}