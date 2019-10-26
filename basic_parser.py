# Parser function body
def __parse_response__(response):
    __CODE_MAPPER = {
        200: 0,
        201: 1,
        400: 2
    }

    segments = response.split(b':')

    code = int(segments[0])
    del segments[0]

    msg = b':'.join(segments)
    return msg, __CODE_MAPPER[code]
