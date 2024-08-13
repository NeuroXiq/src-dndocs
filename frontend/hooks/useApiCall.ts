'use client';
import { useEffect, useState } from "react";
import { useContext } from 'react';
import { GlobalAppContext } from "./globalAppContext";

const ExecApiCall = function (action: any, onCompletedOk: any, gctx: any) : any {
    let cancel = false;

    try {
        let promise = action();    
        
        promise.then((r: any) => {
            if (cancel) {
                return;
            }

            console.log(r);

            onCompletedOk(r);

        }).catch((e: any) => {
            gctx.onUseApiCallPromiseCatch(e);
        });

    } catch (err) {
        gctx.onUseApiCallPromiseCatch(err);
        if (cancel) {
            return;
        }
    } finally {

    }

    return { fnCancel: () => { cancel = true } };
}

export function apiCallNoEffect() {
    const gctx = useContext<any>(GlobalAppContext);

    return function (action: any) {
        var pres, prej;
        var prom = new Promise(function (res, reje) {
            pres = res;
            prej = reje;
        });

        ExecApiCall(action, pres, gctx);

        prom.then((r: any) => {
            gctx.showAjaxGlobalError();
        });

        return prom;
    }
}

export default function useApiCallEffect(action: any) {
    const [result, setResult] = useState<any>(null);
    const [error, setError] = useState<any>(null);
    const [loading, setLoading] = useState<any>(true);
    const [response, setResponse] = useState<any>(true);
    const gctx = useContext<any>(GlobalAppContext);

    useEffect(() => {
        const { fnCancel } = ExecApiCall(action, (r: any) => {
            setResult(r.result);
            setLoading(false);
            setResponse(r);
        }, gctx);

        return () => fnCancel;
    }, []);

    return { result, error, loading, response };
}