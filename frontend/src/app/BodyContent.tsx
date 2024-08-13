import PageLoading from "@/components/PageLoading";
import CookieConsent from "@/components/Shared/CookieConsent";
import Footer from "@/components/footer";
import Navbar from "@/components/navbar";
import { GlobalAppContext, IGlobalAppContextValue } from "hooks/globalAppContext";
import Head from "next/head";
import { usePathname } from "next/navigation";
import { useContext } from "react";

export default function BodyContent(props: any) {
    const gac = useContext<IGlobalAppContextValue>(GlobalAppContext);
    const pathname = usePathname();
    let inBody = null;

    if (pathname === '/' || pathname === '/home') {
        inBody = props.children;
    } else {
        inBody = (
            <>
                <Navbar />
                <main>{props.children}</main>
                <Footer></Footer>
            </>
        );
    }

    if (!gac.user) {
        // need to wait to init user
        // must return body - error if not
        return (<body><div></div></body>);
    }

    return (
        <>
            <body>
                {inBody}
                <CookieConsent />
            </body>
        </>
    );
}