/**
 * Маска телефона: при вводе 8 в начале подставляется +7, формат +7 (XXX) XXX-XX-XX.
 * В БД сохраняется как +79001112242.
 */

(function () {
    'use strict';

    function digitsOnly(str) {
        return (str || '').replace(/\D/g, '');
    }

    function toRawDigits(str) {
        var d = digitsOnly(str);
        if (d.charAt(0) === '8') d = '7' + d.slice(1);
        if (d.charAt(0) !== '7') d = '7' + d;
        return d.slice(0, 11);
    }

    function formatFromDigits(digits) {
        if (digits.length === 0) return '';
        var s = '+7';
        if (digits.length > 1) {
            var rest = digits.slice(1);
            s += ' (' + rest.slice(0, 3);
            if (rest.length > 3) s += ') ' + rest.slice(3, 6);
            if (rest.length > 6) s += '-' + rest.slice(6, 8);
            if (rest.length > 8) s += '-' + rest.slice(8, 10);
        }
        return s;
    }

    /**
     * Возвращает номер для отправки на сервер: +79001112242
     */
    window.getPhoneRaw = function (value) {
        var d = toRawDigits(value || '');
        return d.length === 11 ? '+' + d : '';
    };

    /**
     * Форматирует номер для отображения: +7 (900) 111-22-42
     */
    window.formatPhoneDisplay = function (value) {
        var d = toRawDigits(value || '');
        return formatFromDigits(d);
    };

    function onPhoneInput(input) {
        var digits = toRawDigits(input.value);
        input.value = formatFromDigits(digits);
    }

    function onPhonePaste(e, input) {
        e.preventDefault();
        var text = (e.clipboardData || window.clipboardData).getData('text');
        var digits = toRawDigits(text);
        input.value = formatFromDigits(digits);
    }

    /**
     * Подключить маску к полю (или к элементу по селектору)
     */
    window.applyPhoneMask = function (selectorOrElement) {
        var el = typeof selectorOrElement === 'string'
            ? document.querySelector(selectorOrElement)
            : selectorOrElement;
        if (!el) return;
        el.addEventListener('input', function () { onPhoneInput(el); });
        el.addEventListener('paste', function (e) { onPhonePaste(e, el); });
    };

    function initMasks() {
        document.querySelectorAll('.js-phone-mask').forEach(function (input) {
            if (input.tagName === 'INPUT' && (input.type === 'tel' || input.type === 'text')) {
                window.applyPhoneMask(input);
                if (input.value) onPhoneInput(input);
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initMasks);
    } else {
        initMasks();
    }
})();
