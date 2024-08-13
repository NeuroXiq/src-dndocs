import { TextField } from "@mui/material";
import { getFieldErrors, getInputValue } from "./FormHelpers";

export default function InputText(props: any) {
    let error = getFieldErrors(props.name, props.errors, props.errorsFn);
    let value = getInputValue(props.name, props.value, props.valueFn);

    function onChange(e: any) {
        if (props.stateObj && props.stateObj.setValueObj) {
            props.stateObj.setValueObj({...props.stateObj.valueObj, [props.name]: e.target.value})
        }

        if (props.onChange) {
            props.onChange({
                newValue: e.target.value,
                name: props.name
            });
        }
    }

    return (
        <TextField
            name={props.name}
            disabled={props.disabled}
            error={!!error}
            helperText={error || props.helperText}
            onChange={onChange}
            margin="normal"
            variant="outlined"
            fullWidth
            id={props.id}
            type="text"
            label={props.label}
            value={value} />
        );
}