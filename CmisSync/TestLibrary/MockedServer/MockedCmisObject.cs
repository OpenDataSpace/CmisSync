
namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    public abstract class MockedCmisObject<T> : Mock<T>, IContentChangeEventNotifier where T: class, ICmisObject {
        private DateTime? creationDate = DateTime.UtcNow;
        private DateTime? lastModificationDate = DateTime.UtcNow;
        private string name;
        private MockedAcl Acl;

        public event ContentChangeEventHandler ContentChanged;

        public MockedCmisObject(string name, string id = null, MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.name = name;
            this.Id = id ?? Guid.NewGuid().ToString();
            this.Setup(m => m.CreationDate).Returns(() => this.creationDate);
            this.Setup(m => m.LastModificationDate).Returns(() => this.lastModificationDate);
            this.Setup(m => m.CreatedBy).Returns(() => this.CreatedBy);
            this.Setup(m => m.Id).Returns(() => this.Id);
            this.Setup(m => m.Name).Returns(() => this.name);
            this.Setup(m => m.ObjectType).Returns(() => this.ObjectType);
            this.Setup(m => m.BaseType).Returns(() => this.ObjectType.GetBaseType());
            this.Setup(m => m.BaseTypeId).Returns(() => this.ObjectType.BaseTypeId);
            this.Setup(m => m.ChangeToken).Returns(() => this.ChangeToken);
            this.Setup(m => m.UpdateProperties(It.IsAny<IDictionary<string, object>>())).Callback<IDictionary<string, object>>((dict) => this.UpdateProperties(dict)).Returns(this.Object);
            this.Setup(m => m.UpdateProperties(It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>())).Callback<IDictionary<string, object>, bool>((dict, refresh) => this.UpdateProperties(dict)).Returns(() => Mock.Of<IObjectId>(o => o.Id == this.Id));
            this.SetupRename();
            this.Setup(m => m.SecondaryTypes).Returns(() => new List<ISecondaryType>(this.SecondaryTypes));
            this.Acl = new MockedAcl(behavior);
            this.Setup(m => m.Acl).Returns(() => this.Acl.Object);
        }

        public DateTime? CreationDate {
            get {
                return this.creationDate;
            }

            set {
                this.creationDate = value;
            }
        }

        public DateTime? LastModificationDate {
            get {
                return this.lastModificationDate;
            }

            set {
                this.lastModificationDate = value;
            }
        }

        protected virtual void UpdateProperties(IDictionary<string, object> props) {
            bool updated = false;
            bool updateModificationDateIfUpdated = true;
            foreach (var prop in props) {
                switch (prop.Key) {
                    case PropertyIds.Name:
                    if (!(prop.Value is string)) {
                        throw new ArgumentException("Given name is not a string, but a " + prop.Value.GetType().ToString());
                    }

                    var newName = prop.Value as string;
                    if (newName != this.name) {
                        this.name = newName;
                        updated = true;
                    }

                    break;
                    case PropertyIds.LastModificationDate:
                    if (!(prop.Value.GetType() == typeof(DateTime))) {
                        throw new ArgumentException("Given modification date is not a DateTime, but a " + prop.Value.GetType().ToString());
                    }

                    var newDate = (DateTime)prop.Value;
                    if (newDate != this.lastModificationDate) {
                        this.lastModificationDate = newDate;
                        updated = true;
                        updateModificationDateIfUpdated = false;
                    }

                    break;
                }
            }

            if (updated) {
                if (updateModificationDateIfUpdated) {
                    this.lastModificationDate = DateTime.UtcNow;
                }

                this.UpdateChangeToken();
                this.NotifyChanges();
            }
        }

        protected virtual void UpdateChangeToken() {
            this.ChangeToken = Guid.NewGuid().ToString();
        }

        protected virtual void NotifyChanges(ChangeType? changeType = ChangeType.Updated) {
            var handler = this.ContentChanged;
            if (handler != null) {
                handler(this, Mock.Of<IChangeEvent>(e => e.ObjectId == this.Id && e.ChangeType == changeType && e.ChangeTime == DateTime.UtcNow));
            }
        }

        protected virtual void SetupRename() {
            this.Setup(m => m.Rename(It.IsAny<string>())).Callback<string>(s => { this.Name = s; this.UpdateChangeToken(); this.NotifyChanges(); }).Returns(() => this.Object);
            this.Setup(m => m.Rename(It.IsAny<string>(), It.IsAny<bool>())).Callback<string, bool>((s, b) => { this.Name = s; this.UpdateChangeToken(); this.NotifyChanges(); }).Returns(() => Mock.Of<IObjectId>((o) => o.Id == this.Id));
        }

        protected List<ISecondaryType> SecondaryTypes { get; set; }

        public string CreatedBy { get; set; }

        public IObjectType ObjectType { get; protected set; }
        public string Id { get; protected set; }
        public string ChangeToken { get; set; }
    }
}