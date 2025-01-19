export interface IUser {
    isAuthenticated: boolean,
    jwtInfo: string,
    isAdmin: boolean
}

export default function UseUserService() {
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

    function isAdmin() : any {
        if (!isAuthenticatedf()) {
            return false;
        }

        let jwt = getJwtInfo();

        return jwt.Robinia_IsAdmin === 'true';
    }

    function login(code: string) {
        localStorage.setItem('jwt', code);
    }

    function logout() {
        localStorage.setItem('jwt', '');
    }

    // return { isAuthenticated, isAdmin, loginUser };

    return { login, logout, isAdmin };
}