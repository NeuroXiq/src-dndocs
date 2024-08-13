import { Checkbox, FormControl, FormControlLabel, FormHelperText } from "@mui/material";
import { getInputValue } from "./FormHelpers";

export default function InputCheckbox(props: any) {
    let value = getInputValue(props.name, props.value, props.valueFn)

    function onChange(e: any) {
        props.onChange?.({
            newValue: e.target.checked,
            name: props.name
        });
    }

    return (
        <FormControl>
            <FormControlLabel label={props.label} control={
                <Checkbox
                    name={props.name}
                    checked={value}
                    onChange={onChange} />} />

            {props.helperText && <FormHelperText>{props.helperText}</FormHelperText>}
        </FormControl>
    );
}