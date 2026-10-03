self.addEventListener("install", (event) => {
    //alert("Service Worker installed");
    self.skipWaiting();
});

self.addEventListener("activate", (event) => {
    //alert("Service Worker activated");
});

self.addEventListener("fetch", (event) => {
    // Optional: Add caching here later
});
