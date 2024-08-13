'use client'
import { useEffect, useState } from "react";

export interface IUser {
    isAuthenticated: boolean,
    jwtInfo: string,
    isAdmin: boolean
}

export default function UseUser() {
    const [sLocalStorage, setSLocalStorage] = useState<any>(null);
    const [user, setUser] = useState<IUser>({
        isAuthenticated: false,
        jwtInfo: '',
        isAdmin: false
    });

    const isAuthenticatedf = function (): boolean {
        const jwt = getJwtInfo();
        let authOk = false;

        if (jwt) {
            authOk = jwt.exp > Date.now() / 1000;
        }

        return authOk;
    }

    const getJwtInfo = function (): any {
        const jwt = typeof window !== "undefined" ? localStorage.getItem('jwt') : null;

        if (!jwt) {
            return null;
        }
        // A JWT has 3 parts separated by '.'
        // The middle part is a base64 encoded JSON
        // decode the base64 
        const a = atob(jwt.split(".")[1])
        const obj = JSON.parse(a);

        return obj;
    }

    const isAdmin = function (): any {
        if (!isAuthenticatedf()) {
            return false;
        }

        let jwt = getJwtInfo();

        return jwt.Robinia_IsAdmin === 'true';
    }

    useEffect(() => {
        setSLocalStorage(localStorage);
        refreshUserState();
    }, []);

    function refreshUserState() {
        let newstate = {
            jwtInfo: getJwtInfo(),
            isAdmin: isAdmin(),
            isAuthenticated: isAuthenticatedf(),
        };

        setUser(newstate);
    }

    function login(code: string) {
        localStorage.setItem('jwt', code);
        refreshUserState();
    }

    function logout() {
        localStorage.setItem('jwt', '');
        refreshUserState();
    }

    // return { isAuthenticated, isAdmin, loginUser };

    return { user, login, logout };
}