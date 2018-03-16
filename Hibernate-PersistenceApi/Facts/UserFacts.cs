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

        /*
         * 1. update transient user
         * 2. update persistent user
         * 3. update detached user
         */
        [Fact]
        public void should_update_user()
        {
            var user = new User {Name = "Zhu"};
            using (ISession session = OpenSession())
            {
                session.Update(user);
            }

            using (ISession session = OpenSession())
            {
                session.Save(user);
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                user.Name = "Zhen";
                //session.Update(user);
                session.Flush();
                Assert.Equal("Zhen", user.Name);
            }

            var detachedUser = new User { Id = user.Id, Name = "Zhu" };
            using (ISession session = OpenSession())
            {
                detachedUser.Name = "Guang";
                session.Flush();
            }

            using (ISession session = OpenSession())
            {
                session.Update(detachedUser);
                Assert.Equal("Zhu", detachedUser.Name);

                detachedUser.Name = "Guang";
                session.Flush();
                Assert.Equal("Guang", detachedUser.Name);
            }
        }

        /*
         * 1. save or update transient user
         * 2. save or update detached user
         */
        [Fact]
        public void should_save_or_update_user()
        {
            var user = new User {Name = "Zhu"};
            using (ISession session = OpenSession())
            {
                session.SaveOrUpdate(user);
                Assert.NotEqual(0, user.Id);
            }
            
            using (ISession session = OpenSession())
            {
                session.SaveOrUpdate(user);

                user.Name = "Zhen";
                session.Flush();
                Assert.Equal("Zhen", user.Name);
            }
        }

        [Fact]
        public void should_lock_user()
        {
            var user = new User {Name = "Zhu"};
            using (ISession session = OpenSession())
            {
                //session.Lock(user, LockMode.None);
            }

            using (ISession session = OpenSession())
            {
                session.Save(user);
                session.Lock(user, LockMode.None);
            }

            using (ISession session = OpenSession())
            {
                session.Lock(user, LockMode.None);

                user.Name = "Zhen";
                session.Flush();
            }
        }
    }
}