using System;

namespace DreamAmazon
{
    public class Account
    {
        private String _email;
        public String Email
        {
            get { return _email; }
            protected set { _email = value; }
        }

        private String _pass;
        public String Password
        {
            get { return _pass; }
            protected set { _pass = value; }
        }

        private String _gc;
        public String GiftCardBalance
        {
            get { return _gc; }
            set { _gc = value; }
        }

        private String _zip;
        public String ZipCode
        {
            get { return _zip; }
            set { _zip = value; }
        }

        private String _phone;
        public String Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        private String _orders;
        public String Orders
        {
            get { return _orders; }
            set { _orders = value; }
        }        

        public Account(String email, String pass)
        {
            this.Email = email;
            this.Password = pass;
        }  
    }
}
