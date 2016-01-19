using System;
using MyStarStable.Common;
using Reachability;

namespace MyStarStable.iOS
{
    class NetworkTester_iOS : INetworkTester
    {
        public bool Ready;

        private Reachability.Reachability _reachability;
        public NetworkTester_iOS()
        {
            _reachability = new Reachability.Reachability("www.starstable.com");
            _reachability.ReachabilityUpdated += OnReachabilityUpdated;
            
            Ready = false;
        }

        public event EventHandler ReachabilityUpdated;

        private void OnReachabilityUpdated(object sender, ReachabilityEventArgs reachabilityEventArgs)
        {
            Ready = true;
            if (ReachabilityUpdated != null)
            {
                ReachabilityUpdated(this, EventArgs.Empty);
            }
        }

        public bool HasInternet
        {
            get { return _reachability.IsReachable; }
        }
    }
}