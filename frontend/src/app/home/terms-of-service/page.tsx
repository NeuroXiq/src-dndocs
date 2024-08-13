import TermsOfServiceCookies from "@/components/partials/termsOfServiceCookies";
import TermsOfServiceCoreContent from "@/components/partials/termsOfServiceCoreContent";
import TermsOfServiceCustomContent from "@/components/partials/termsOfServiceCustomContent";

export default function TermsOfService() {
    return (
        <>
            <TermsOfServiceCookies />
            <TermsOfServiceCustomContent />
            <TermsOfServiceCoreContent />
        </>
    );
}