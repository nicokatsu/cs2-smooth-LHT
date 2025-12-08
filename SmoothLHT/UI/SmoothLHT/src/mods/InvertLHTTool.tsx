import {ModuleRegistryExtend} from "cs2/modding";
import {bindValue, trigger, useValue} from "cs2/api";
import mod from "../../mod.json"
import styles from "./InvertLHTTool.module.scss"
import buttonImg from '../../imgs/button.svg'
import {VanillaComponentResolver} from "./VanillaComponentResolver";

const isShowing$ = bindValue<boolean>(mod.id, 'IsShowing')
const isInverted$ = bindValue<number>(mod.id, 'IsInverted')

const toggleInverted = (val: number) => {
    trigger(mod.id, 'ToggleInverted', val)
}
export const InvertLHTTool: ModuleRegistryExtend = (Component: any) => {

    return (props) => {
        const results = Component()

        const isShowing = useValue(isShowing$);
        const isInverted = useValue(isInverted$)
        const handleToggle = () => {
            toggleInverted(isInverted ? 0 : 1)
        }

        if (isShowing) {
            results?.props?.children?.push?.(
                <VanillaComponentResolver.instance.Section title="LHT Invert Buiding Networks">
                    <VanillaComponentResolver.instance.ToolButton selected={isInverted === 1} onSelect={handleToggle}
                                                                  src={buttonImg}
                                                                  tooltip="Changes apply to all existing instances of this building."
                                                                  focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                                                                  className={VanillaComponentResolver.instance.toolButtonTheme.ToolButton}><label
                        className={styles.centeredContentButton}></label></VanillaComponentResolver.instance.ToolButton>

                </VanillaComponentResolver.instance.Section>
            )

        }
        return results

    }

}