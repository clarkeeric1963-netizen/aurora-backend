/* Aurora TMS — shared version badge. Reads /version.json and drops a pill into the top bar.
   Include once per page: <script src="/version-badge.js"></script> */
(function () {
  fetch("/version.json", { cache: "no-store" })
    .then(function (r) { return r.json(); })
    .then(function (v) {
      var badge = document.createElement("div");
      badge.title = "Build version";
      badge.style.cssText =
        "display:inline-flex;align-items:center;gap:8px;margin-left:12px;padding:5px 12px;" +
        "border-radius:20px;border:1px solid rgba(148,163,184,.4);background:rgba(148,163,184,.15);" +
        "font-size:14px;line-height:1;white-space:nowrap;font-family:inherit;";
      var ver = document.createElement("span");
      ver.textContent = "v" + (v.version || "?");
      ver.style.cssText = "color:#3b82f6;font-weight:700;";
      badge.appendChild(ver);
      if (v.built) {
        var sep = document.createElement("span"); sep.textContent = "|"; sep.style.cssText = "color:#94a3b8;";
        var dt = document.createElement("span"); dt.textContent = String(v.built).replace(/-/g, ".");
        dt.style.cssText = "color:#94a3b8;font-weight:500;font-size:12px;";
        badge.appendChild(sep); badge.appendChild(dt);
      }
      var anchor = document.querySelector(".brand") || document.querySelector(".ttl");
      if (anchor && anchor.parentNode) {
        anchor.parentNode.insertBefore(badge, anchor.nextSibling);
      } else {
        badge.style.position = "fixed"; badge.style.top = "8px"; badge.style.right = "12px"; badge.style.zIndex = "9999";
        document.body.appendChild(badge);
      }
    })
    .catch(function () { /* no version.json yet — show nothing */ });
})();
