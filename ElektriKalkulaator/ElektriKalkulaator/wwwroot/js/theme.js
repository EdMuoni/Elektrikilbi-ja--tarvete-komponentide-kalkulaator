/* =============================================================================
   theme.js — remembers whether the visitor prefers light or dark
   =============================================================================
   Three states, matching theme.css:

     "system" — no attribute set; the operating system decides (the default)
     "light"  — the visitor chose light
     "dark"   — the visitor chose dark

   The choice is kept in localStorage, so it survives closing the browser. It is
   stored per browser rather than per account on purpose: someone may want dark
   on their phone and light on a bright workshop laptop while signed in as the
   same person.

   IMPORTANT: a small copy of the "read the saved choice and set the attribute"
   step also runs inline in <head>, BEFORE the stylesheets load. See _Layout.cshtml.
   Without that, the page paints in the default theme first and then switches,
   which the visitor sees as a flash of the wrong colours on every page load.
   ============================================================================= */
(function () {
    'use strict';

    var STORAGE_KEY = 'ek-theme';

    /* Reading localStorage can throw — private browsing modes and some corporate
       policies block it. A failure here must never stop the rest of the page
       working, so every access is wrapped. */
    function readSavedTheme() {
        try {
            return localStorage.getItem(STORAGE_KEY);
        } catch (e) {
            return null;
        }
    }

    function saveTheme(value) {
        try {
            if (value === null) {
                localStorage.removeItem(STORAGE_KEY);
            } else {
                localStorage.setItem(STORAGE_KEY, value);
            }
        } catch (e) {
            /* Nothing we can do; the choice simply will not be remembered. */
        }
    }

    /* What the visitor is looking at RIGHT NOW, which is not the same as what
       they have chosen: with no explicit choice, it is whatever the OS asks for. */
    function currentlyShowing() {
        var chosen = document.documentElement.getAttribute('data-theme');
        if (chosen === 'light' || chosen === 'dark') {
            return chosen;
        }
        return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    }

    function applyTheme(value) {
        if (value === 'light' || value === 'dark') {
            document.documentElement.setAttribute('data-theme', value);
        } else {
            /* Removing the attribute hands control back to the operating system. */
            document.documentElement.removeAttribute('data-theme');
        }
        updateToggleButton();
    }

    /* Keeps the button's icon, label and accessible state in step with reality. */
    function updateToggleButton() {
        var button = document.getElementById('themeToggle');
        if (!button) return;

        var showing = currentlyShowing();
        var goingTo = showing === 'dark' ? 'light' : 'dark';

        /* The icon shows what you will GET, not what you are looking at — the
           common alternative confuses people into clicking the wrong way. */
        button.textContent = goingTo === 'light' ? '☀️' : '🌙';
        button.setAttribute(
            'title',
            goingTo === 'light' ? 'Lülita hele taust' : 'Lülita tume taust'
        );
        button.setAttribute('aria-label', button.getAttribute('title'));
        button.setAttribute('aria-pressed', showing === 'light' ? 'true' : 'false');
    }

    function toggle() {
        var next = currentlyShowing() === 'dark' ? 'light' : 'dark';
        saveTheme(next);
        applyTheme(next);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var button = document.getElementById('themeToggle');
        if (button) {
            button.addEventListener('click', toggle);
        }
        updateToggleButton();
    });

    /* If the visitor has made no explicit choice and changes their OS setting
       (many systems switch automatically at sunset), follow it live. */
    window.matchMedia('(prefers-color-scheme: light)').addEventListener('change', function () {
        if (!readSavedTheme()) {
            updateToggleButton();
        }
    });
})();
