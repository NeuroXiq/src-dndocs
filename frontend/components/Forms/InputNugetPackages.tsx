import { FormControl, FormHelperText, InputLabel, OutlinedInput } from "@mui/material";
import { getFieldErrors, getInputValue } from "./FormHelpers";
import NugetPackageModel from "@/services/APIModel/Shared/NugetPackageModel";

export default function InputNugetPackages(props: any) {
    const error = getFieldErrors(props.name, props.errors, props.errorsFn);
    const disabled = props.disabled;
    const name = props.name || "nugetpackages";
    const id =  props.id || "nugetpackages";
    let value = getInputValue(props.name, props.value, props.valueFn);

    if (Array.isArray(value)) {
        value = value.join('\r\n');
    }
    
    function onChange(e: any) {
        let event = {
            e: e,
            newValue: e.target.value,
            name: name
        };

        props.onChange && props.onChange(event);
    }

    return (
    <FormControl fullWidth>
        <InputLabel htmlFor="nugetpackages">{props.label}</InputLabel>
        <OutlinedInput id={id}
            multiline
            fullWidth
            placeholder={props.placeholder}
            value={value}
            name={name}
            label={props.label}
            rows={8}
            onChange={onChange}
            disabled={disabled} />
        {!!error && <FormHelperText error={true}>{error}</FormHelperText>}
        <FormHelperText>
            If project has more than one Nuget Package You can add multiple packages separated by a new line.<br/>
            When form/backend does not require version, do not provide version only package name, e.g. Arctium.Shared.<br/>
            When form/backend require version, add version with package name e.g. Arctium.Shared 1.2.3
        </FormHelperText>
    </FormControl>);
}

function NupkgStringToArray(nugetpackages: any): NugetPackageModel[] {
    let nugetpackagesArray: NugetPackageModel[] = [];

    if (nugetpackages && nugetpackages.length) {
        let names = nugetpackages.split(/\r?\n/)
            .map((r: any) => r?.trim())
            .filter((r: any) => !!r && r.length > 0);

        names.forEach((v: string) => {
            v = v?.trim();
            let pkgdata = v?.split(' ');

            if (pkgdata && pkgdata.length > 0) {
                const identityid = pkgdata[0];
                const identityver = pkgdata.length > 1 ? pkgdata[1] : null;

                let val: NugetPackageModel = {
                    identityId: identityid,
                    identityVersion: identityver
                }

                nugetpackagesArray.push(val);
            }
        });
    }

    return nugetpackagesArray;
}

export { NupkgStringToArray }