import { ModRegistrar, ModuleRegistry } from "cs2/modding";
import { createInvertLHTTool, ToolOptionsComponents } from "./mods/InvertLHTTool";

const MOUSE_TOOL_OPTIONS_MODULE = "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx";
const TOOL_BUTTON_MODULE = "game-ui/game/components/tool-options/tool-button/tool-button.tsx";
const TOOL_BUTTON_THEME_MODULE = "game-ui/game/components/tool-options/tool-button/tool-button.module.scss";

function getRegistryExport<T>(moduleRegistry: ModuleRegistry, modulePath: string, exportName: string): T {
    const resolvedValue = moduleRegistry.get(modulePath, exportName);
    if (!resolvedValue) {
        throw new Error(`[SmoothLHT] Missing vanilla UI export ${exportName} from ${modulePath}`);
    }

    return resolvedValue as T;
}

function resolveToolOptionsComponents(moduleRegistry: ModuleRegistry): ToolOptionsComponents {
    const toolButtonTheme = moduleRegistry.get(TOOL_BUTTON_THEME_MODULE, "classes") as { ToolButton?: string } | undefined;

    return {
        Section: getRegistryExport(moduleRegistry, MOUSE_TOOL_OPTIONS_MODULE, "Section"),
        ToolButton: getRegistryExport(moduleRegistry, TOOL_BUTTON_MODULE, "ToolButton"),
        toolButtonClassName: toolButtonTheme?.ToolButton,
    };
}

const register: ModRegistrar = (moduleRegistry) => {
    const toolOptionsComponents = resolveToolOptionsComponents(moduleRegistry);
    moduleRegistry.extend(
        MOUSE_TOOL_OPTIONS_MODULE,
        "MouseToolOptions",
        createInvertLHTTool(toolOptionsComponents)
    );
};

export default register;
