import { FormControlLabel, FormLabel, Radio, RadioGroup } from "@mui/material";
import { getInputValue } from "./FormHelpers";

export default function InputRadioGroup(props: any) {
    let value = getInputValue(props.name, props.value, props.valueFn);
    let id = props.id || props.name;
    const labelId = Math.random().toString();

    function onChange(e: any) {
        props.onChange?.({ name: props.name, newValue: e.target.value });
    }

    return (
        <>
            <FormLabel sx={{mt: 1}} id={labelId}>{props.label}</FormLabel>
            <RadioGroup
                aria-labelledby={labelId}
                id={id}
                onChange={onChange}
                row={props.row}
                name={props.name}
                value={value}>
                {props.radioButtons.map((btn: any) => {
                    if (!btn.value) {
                        throw new Error('btn does not have value, label: ' + btn.label);
                    }

                    return (
                        <FormControlLabel key={btn.value} value={btn.value} control={<Radio />} label={btn.label} />
                    );
                })}
            </RadioGroup>
        </>
    )
}