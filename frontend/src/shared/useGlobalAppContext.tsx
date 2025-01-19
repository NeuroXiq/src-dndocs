import { useContext } from "react";
import { GlobalAppContext, IGlobalAppContextValue } from "./GlobalAppContext";

export default function useGlobalAppContext() {
    return useContext<IGlobalAppContextValue>(GlobalAppContext);
}