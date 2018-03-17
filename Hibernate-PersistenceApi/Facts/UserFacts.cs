using System;
using System.Collections.Generic;
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
				VerifyUserName(userId, "Zhu");
			}

			using (ISession session = OpenSession())
			{
				user = session.Load<User>(user.Id);
				user.Name = "Zhen";
				session.Save(user);
				VerifyUserName(user.Id, "Zhu");
			}

			using (ISession session = OpenSession())
			{
				user.Name = "Guang";
				session.Save(user);
				VerifyUserName(user.Id, "Zhu");
				VerifyUsersCount(2);
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
				VerifyUserName(user.Id, "Zhu");
			}

			using (ISession session = OpenSession())
			{
				user = session.Load<User>(user.Id);
				user.Name = "Zhen";
				session.Persist(user);
				VerifyUserName(user.Id, "Zhu");
			}

			using (ISession session = OpenSession())
			{
				user.Name = "Guang";
				var exception = Assert.Throws<PersistentObjectException>(() => session.Persist(user));
				Assert.Equal("detached entity passed to persist: Hibernate_PersistenceApi.User", exception.Message);
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
				VerifyUsersCount(0);

				var exception = Assert.Throws<StaleStateException>(() => session.Flush());
				Assert.Equal(
					"Batch update returned unexpected row count from update; actual row count: 0; expected: 1",
					exception.Message);
			}

			using (ISession session = OpenSession())
			{
				session.Save(user);

				user.Name = "Zhen";
				session.Flush();
				VerifyUserName(user.Id, "Zhen");
			}

			var detachedUser = new User {Id = user.Id, Name = "Guang"};
			using (ISession session = OpenSession())
			{
				session.Update(detachedUser);

				session.Flush();
				VerifyUserName(detachedUser.Id, "Guang");
			}
		}

		/*
		 * 1. save or update transient user
		 * 2. save or update persistent user
		 * 3. save or update detached user
		 */
		[Fact]
		public void should_save_or_update_user()
		{
			var user = new User {Name = "Zhu"};
			using (ISession session = OpenSession())
			{
				session.SaveOrUpdate(user);
				Assert.NotEqual(0, user.Id);
				VerifyUserName(user.Id, "Zhu");
			}

			using (ISession session = OpenSession())
			{
				user = session.Load<User>(user.Id);
				user.Name = "Zhen";

				session.SaveOrUpdate(user);
				VerifyUserName(user.Id, "Zhu");
			}

			using (ISession session = OpenSession())
			{
				session.SaveOrUpdate(user);

				user.Name = "Guang";
				session.Flush();
				Assert.Equal("Guang", user.Name);
			}
		}

		[Fact]
		public void should_lock_user()
		{
			var user = new User {Name = "Zhu"};
			using (ISession session = OpenSession())
			{
				var exception = Assert.Throws<TransientObjectException>(() => session.Lock(user, LockMode.None));
				Assert.Equal("cannot lock an unsaved transient instance: Hibernate_PersistenceApi.User", exception.Message);
			}

			using (ISession session = OpenSession())
			{
				session.Save(user);
			}

			using (ISession session = OpenSession())
			{
				session.Lock(user, LockMode.None);

				user.Name = "Zhen";
				session.Flush();
				VerifyUserName(user.Id, "Zhen");
			}
		}

		/*
		 * 1. merge transient user
		 * 2. merge persistent user
		 * 3. merge detached user
		 */
		[Fact]
		public void should_merge_user()
		{
			var user = new User {Name = "Zhu"};
			using (ISession session = OpenSession())
			{
				User persistentUser = session.Merge(user);
				Assert.Equal(0, user.Id);
				Assert.NotEqual(0, persistentUser.Id);
				VerifyUserName(persistentUser.Id, "Zhu");
			}

			using (ISession session = OpenSession())
			{
				user = session.QueryOver<User>().SingleOrDefault();
				User anotherPersistentUser = session.Merge(user);
				Assert.Same(user, anotherPersistentUser);
			}

			var request = new User
			{
				Id = user.Id,
				Name = "Zhen"
			};

			using (ISession session = OpenSession())
			{
				user = session.Load<User>(user.Id);
				Console.WriteLine($"original user name is {user.Name}");

				var exception = Assert.Throws<NonUniqueObjectException>(() => session.Update(request));
				Assert.Equal(
					"a different object with the same identifier value was already associated with the session: 1, of entity: Hibernate_PersistenceApi.User", 
					exception.Message);
			}

			using (ISession session = OpenSession())
			{
				user = session.Load<User>(user.Id);
				Console.WriteLine($"original user name is {user.Name}");

				session.Merge(request);
				session.Flush();

				VerifyUserName(user.Id, "Zhen");
			}
		}

		void VerifyUserName(long id, string expectedName)
		{
			using (ISession session = OpenSession())
			{
				var user = session.Load<User>(id);
				Assert.Equal(expectedName, user.Name);
			}
		}

		void VerifyUsersCount(int count)
		{
			using (ISession session = OpenSession())
			{
				IList<User> users = session.QueryOver<User>().List();
				Assert.Equal(count, users.Count);
			}
		}
	}
}