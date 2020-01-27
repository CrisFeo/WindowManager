using System;
using System.Collections.Generic;
using System.Linq;
using WinCtl;

using W = WinCtl.Window;
using H = WinCtl.Hotkey;
using M = WinCtl.Hotkey.Mod;
using K = WinCtl.Key;
using G = WinCtl.Graphics;
using Color = WinCtl.Graphics.Color;

static class User {

  // Constants
  ///////////////////////

  const int GAP_SIZE = 20;

  const int BORDER_SIZE = 8;
  const int BORDER_OFFSET = 0;
  static readonly Color BORDER_COLOR = new Color(95, 135, 0);
  static readonly HashSet<string> BORDER_IGNORE_TITLES = new HashSet<string> {
    "Cortana",
  };

  const long TAP_DURATION = 100;

  const M MOD_PUSH = M.Win | M.Shift;
  const M MOD_FOCUS = M.Win;
  const M MOD_SWITCH = M.Win;

  // Methods
  ///////////////////////

  static void Main() {
    VirtualDesktop();
    WindowArrangement();
    WindowFocus();
    WindowBorder();
    ShiftParentheses();
    TabAlt();
    CapsControl();
  }

  static void VirtualDesktop() {
    Map(MOD_SWITCH, K.N1, () => Desktop.GoTo(0));
    Map(MOD_SWITCH, K.N2, () => Desktop.GoTo(1));
    Map(MOD_SWITCH, K.N3, () => Desktop.GoTo(2));
    Map(MOD_SWITCH, K.N4, () => Desktop.GoTo(3));
    Map(MOD_SWITCH, K.N5, () => Desktop.GoTo(4));
    Map(MOD_SWITCH, K.N6, () => Desktop.GoTo(5));
    Map(MOD_SWITCH, K.N7, () => Desktop.GoTo(6));
    Map(MOD_SWITCH, K.N8, () => Desktop.GoTo(7));
    Map(MOD_SWITCH, K.N9, () => Desktop.GoTo(8));
  }

  static void WindowArrangement() {
    var g = GAP_SIZE;
    var hg = GAP_SIZE / 2;
    var ghg = GAP_SIZE + hg;
    Map(MOD_PUSH, K.Y, (a, w, h) => W.Move(a, 0,         0,         w,       h     ));
    Map(MOD_PUSH, K.U, (a, w, h) => W.Move(a, g,         g,         w-2*g,   h-2*g ));
    Map(MOD_PUSH, K.I, (a, w, h) => W.Move(a, (w-a.w)/2, (h-a.h)/2, null,    null  ));
    Map(MOD_PUSH, K.H, (a, w, h) => W.Move(a, g,         null,      w/2-ghg, null  ));
    Map(MOD_PUSH, K.L, (a, w, h) => W.Move(a, w/2+hg,    null,      w/2-ghg, null  ));
    Map(MOD_PUSH, K.K, (a, w, h) => W.Move(a, null,      g,         null,    h/2-ghg));
    Map(MOD_PUSH, K.J, (a, w, h) => W.Move(a, null,      h/2+hg,    null,    h/2-ghg));
  }

  static void WindowFocus() {
    Map(MOD_FOCUS, K.H, a => W.SetActive(W.All()
      .Where(w => w.isVisible)
      .Where(w => w.x < a.x)
      .OrderBy(w => w.x)
      .ThenBy(w => Math.Abs(a.y - w.y))
      .DefaultIfEmpty(a)
      .First()));
    Map(MOD_FOCUS, K.L, a => W.SetActive(W.All()
      .Where(w => w.isVisible)
      .Where(w => w.x > a.x)
      .OrderByDescending(w => w.x)
      .ThenBy(w => Math.Abs(a.y - w.y))
      .DefaultIfEmpty(a)
      .First()));
    Map(MOD_FOCUS, K.K, a => W.SetActive(W.All()
      .Where(w => w.isVisible)
      .Where(w => w.y < a.y)
      .OrderBy(w => w.y)
      .ThenBy(w => Math.Abs(a.x - w.x))
      .DefaultIfEmpty(a)
      .First()));
    Map(MOD_FOCUS, K.J, a => W.SetActive(W.All()
      .Where(w => w.isVisible)
      .Where(w => w.y > a.y)
      .OrderByDescending(w => w.y)
      .ThenBy(w => Math.Abs(a.x - w.x))
      .DefaultIfEmpty(a)
      .First()));
  }

  static void WindowBorder() {
    var o = BORDER_OFFSET + BORDER_SIZE / 2;
    var activeBorderGraphic = G.New(c => {
      var a = W.Active();
      if (!a.isValid) return;
      if (BORDER_IGNORE_TITLES.Contains(W.Title(a))) return;
      G.Rect(c, BORDER_COLOR, BORDER_SIZE, a.x-o, a.y-o, a.w+2*o, a.h+2*o);
    });
    Event.onFocus += w => G.Redraw(activeBorderGraphic);
    Event.onMove += w => G.Redraw(activeBorderGraphic);
  }

  static void ShiftParentheses() {
    MapTap(
      TAP_DURATION,
      false,
      K.LeftShift,
      new[] { K.LeftShift },
      new[] { K.LeftShift, K.N9 }
    );
    MapTap(
      TAP_DURATION,
      false,
      K.RightShift,
      new[] { K.RightShift },
      new[] { K.RightShift, K.N0 }
    );
  }

  static void TabAlt() {
    MapTap(
      TAP_DURATION,
      true,
      K.Tab,
      new[] { K.LeftMenu },
      new[] { K.Tab }
    );
  }

  static void CapsControl() {
    MapTap(
      TAP_DURATION,
      false,
      K.CapsLock,
      new[] { K.LeftControl },
      new[] { K.Escape }
    );
  }

  // Helper methods
  ///////////////////////

  static void Map(M mod, K key, Action fn) {
    H.MapDown(mod, key, false, fn);
  }

  static void Map(M mod, K key, Action<W.Info> fn) {
    H.MapDown(mod, key, false, () => {
      var active = W.Active();
      if (!active.isValid) return;
      fn(active);
    });
  }

  static void Map(M mod, K key, Action<W.Info, int, int> fn) {
    H.MapDown(mod, key, false, () => {
      var active = W.Active();
      if (!active.isValid) return;
      var (w, h) = W.Resolution();
      fn(active, w, h);
    });
  }

  static void MapTap(
    long tapDuration,
    bool delayHold,
    K from,
    K[] hold,
    K[] tap
  ) {
    long? downTime = null;
    H.MapDown(M.Any, from, true, () => {
      if (downTime.HasValue) return;
      var thisDownTime = downTime = Time.Now();
      if (delayHold) {
        Time.After(tapDuration, () => {
          if (downTime != thisDownTime) return;
          SendRaw(hold.Select(k => (k, true)));
        });
      } else {
        SendRaw(hold.Select(k => (k, true)));
      }
    });
    H.MapUp(M.Any, from, () => {
      if (!downTime.HasValue) return;
      if (Time.Now() - downTime.Value > tapDuration) {
        SendRaw(hold.Select(k => (k, false)));
      } else {
        var tapPress = Enumerable.Concat(
          tap.Select(k => (k, true)),
          tap.Select(k => (k, false))
        );
        if (delayHold) {
          SendRaw(tapPress);
        } else {
          SendRaw(Enumerable.Concat(
            hold.Select(k => (k, false)),
            tapPress
          ));
        }
      }
      downTime = null;
    });
  }

  static void SendRaw(IEnumerable<(K, bool)> keystrokes) {
    Input.SendRaw(new LinkedList<(K, bool)>(keystrokes));
  }

}

