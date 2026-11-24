// Kliensoldali jelek a matek gyakorláshoz (gyenge, kiegészítő információ a szervernek).
// Adatminimalizálás: csak számlálók és logikai értékek. Billentyűleütés-tartalmat, beillesztett szöveget,
// képernyőt vagy kamerát SOHA nem rögzítünk. A jeleket a válasszal együtt küldi a webkliens, majd nullázza.

const state = {
    started: false,
    focusLossMs: 0,
    hiddenSince: null,
    paste: false,
    untrusted: false,
    inputSeen: false,
};

function now() {
    return performance.now();
}

function lostFocus() {
    if (state.hiddenSince === null) state.hiddenSince = now();
}

function gainedFocus() {
    if (state.hiddenSince !== null) {
        state.focusLossMs += now() - state.hiddenSince;
        state.hiddenSince = null;
    }
}

function inAnswerArea(target) {
    return !!(target && target.closest && target.closest('.feladat'));
}

/** A figyelés indítása (többször hívható; az eseménykezelők csak egyszer kerülnek fel). */
export function start() {
    if (!state.started) {
        state.started = true;
        document.addEventListener('visibilitychange', () => (document.hidden ? lostFocus() : gainedFocus()));
        window.addEventListener('blur', lostFocus);
        window.addEventListener('focus', gainedFocus);
        // Csak azt jegyezzük meg, HOGY volt beillesztés a válaszmezőbe; a tartalmát nem olvassuk.
        document.addEventListener('paste', (e) => { if (inAnswerArea(e.target)) state.paste = true; }, true);
        for (const type of ['keydown', 'input', 'click', 'pointerdown', 'mousedown', 'touchstart']) {
            document.addEventListener(type, (e) => {
                if (!e.isTrusted) state.untrusted = true;
                else state.inputSeen = true;
            }, true);
        }
    }
    reset();
}

/** Nullázás (új feladat kezdete). */
export function reset() {
    state.focusLossMs = 0;
    state.hiddenSince = document.hidden || !document.hasFocus() ? now() : null;
    state.paste = false;
    state.untrusted = false;
    state.inputSeen = false;
}

/** Az előző nullázás óta gyűlt jelek, majd nullázás. */
export function take() {
    let focusLoss = state.focusLossMs;
    if (state.hiddenSince !== null) focusLoss += now() - state.hiddenSince;
    const result = {
        focusLossMs: Math.max(0, Math.min(Math.round(focusLoss), 3600000)),
        paste: state.paste,
        webDriver: navigator.webdriver === true,
        untrustedEvent: state.untrusted,
        noInputEvents: !state.inputSeen,
    };
    reset();
    return result;
}
