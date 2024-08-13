import { useContext } from "react";
import { GlobalAppContext, IGlobalAppContextValue } from "./globalAppContext";

export default function useGlobalAppContext() {
    return useContext<IGlobalAppContextValue>(GlobalAppContext);
}