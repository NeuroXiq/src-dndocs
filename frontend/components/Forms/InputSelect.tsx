import { FormControl, FormHelperText, InputLabel, MenuItem, Select } from "@mui/material";
import { getInputValue } from "./FormHelpers";

export default function InputSelect(props: any) {
    if (!props.items) {
        console.error('inputselect: null/undefined items prop');
        throw new Error('null/undefined items prop');
    }

    let value = getInputValue(props.name, props.value, props.valueFn);

    function onChange(e: any) {
        props.onChange?.({
            name: props.name,
            newValue: e.target.value
        });
    }

    function items(): any {
        if (!props.items || props.items.length === 0) {
            return null;
        }

        return (
            props.items.map((i:any) => {
                return (<MenuItem key={i.text} value={i.value}>{i.text}</MenuItem>);
            })
        )
    }

    const id = Math.random().toString();

    return (
        <FormControl fullWidth margin="normal">
            <InputLabel id={id}>{props.label}</InputLabel>
            <Select
                disabled={props.disabled}
                labelId={id}
                label={props.label}
                value={value}
                id="asdf"
                onChange={onChange}>
                {items()}        
            </Select>
        {props.helperText && <FormHelperText>{props.helperText}</FormHelperText>}
        </FormControl>
    );
}