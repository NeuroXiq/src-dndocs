import { useEffect, useRef, useState } from 'react';
import '../../css/cookie-consent.css';
import React from 'react';
import Urls from 'config/Urls';

export default function CookieConsent() {
    const divRef = useRef<any>(null);
    const isFirstRender = useRef(true);

    var consentTrue = !!document.cookie
        .split("; ")
        .find((row) => row.startsWith("DndocsCookiesConsent"));

    useEffect(() => {
        return;

        if (typeof document !== undefined && typeof window !== undefined) {

        } else {
            return;
        }

        if (!divRef.current || !isFirstRender.current || consentTrue) {
            return;
        }

        let cancel = false;

        isFirstRender.current = false;

        fetch(Urls.other.cookiesConsentHtml, { method: 'GET' })
            .then(r => r.text())
            .then(r => {
                if (cancel) {
                    return;
                }

                const slotHtml = document.createRange().createContextualFragment(r)
                divRef.current.innerHtml = ''
                divRef.current.appendChild(slotHtml);
            });
        
        return () => { cancel = true };
    }, [divRef, isFirstRender]);

    return (
        <div ref={divRef}>
        </div>
    );
}