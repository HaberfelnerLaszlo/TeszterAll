// A gyakorlási tájékoztató „Megértettem” állapota ebben a böngészőben (kényelmi beállítás, nem kötelező).
// Ha a tárhely nem érhető el (pl. letiltva), a tájékoztató egyszerűen újra megjelenik.

const KEY = 'teszter.practiceNotice.dismissed';

export function isDismissed() {
    try {
        return window.localStorage.getItem(KEY) === '1';
    } catch {
        return false;
    }
}

export function setDismissed(value) {
    try {
        if (value) window.localStorage.setItem(KEY, '1');
        else window.localStorage.removeItem(KEY);
    } catch {
        // nem baj: legközelebb újra megjelenik
    }
}
